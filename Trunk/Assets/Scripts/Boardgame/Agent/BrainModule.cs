using UnityEngine;
using System.Collections.Generic;
using Boardgame.GDL;
using FML;
using FML.Boardgame;
using Behaviour;
using System.Collections;
using Boardgame.Configuration;
using System;
using UnityEngine.Events;

namespace Boardgame.Agent {
    [RequireComponent(typeof(PersonalityModule), typeof(InputModule), typeof(BehaviourRealiser))]
    public class BrainModule : MonoBehaviour {
        private PersonalityModule pm;
        private InputModule im;
        private BehaviourRealiser behave;
        private ActorMotion motion;
        private Participant me;

        public Player player;

        void Awake() {
            pm = GetComponent<PersonalityModule>();
            im = GetComponent<InputModule>();
            behave = transform.parent.GetComponentInChildren<BehaviourRealiser>();
            me = new Participant();
            me.identikit = GetComponent<Identikit>();
            motion = transform.parent.GetComponentInChildren<ActorMotion>();

            //StartCoroutine(FakeEmotion());
        }

        private MentalChunk getEmotion() {
            MentalChunk chunk = new MentalChunk();
            chunk.AddFunction(new EmotionFunction(pm.GetArousal(), pm.GetValence()));
            chunk.owner = me;
            return chunk;
        }

        IEnumerator FakeEmotion()
        {
            while (true)
            {
                yield return new WaitForSeconds(2f);
                FMLBody body = new FMLBody();
                MentalChunk chunk = new MentalChunk();
                chunk.AddFunction(new EmotionFunction(UnityEngine.Random.Range((float)Config.Low, (float)Config.High), UnityEngine.Random.Range((float)Config.Low, (float)Config.High)));
                chunk.owner = me;
                body.AddChunk(chunk);
                interpret(body);
            }
        }

        Move bestMove;
        Move worstMove;
        Player forWho; //making sure the data is in sync
        enum MoveReaction {
            CONFUSED,
            POSITIVE,
            NEGATIVE,
            NEUTRAL
        }

        private MoveReaction react(Move move, Player who) {
            //our timing was bad :( be neutral to be safe
            if (who != forWho) return MoveReaction.NEUTRAL;

            Debug.Log("Getting reaction");

            if(player == who) {
                if (move.Equals(bestMove)) {
                    if (myUCTavg > foeUCTavg) {
                        //depending on personality might be neut
                        return MoveReaction.POSITIVE;
                    } else { 
                        //depending on personality might be neutral
                        return MoveReaction.NEGATIVE;
                    }
                }

                if(move.Equals(worstMove)) {

                    if (myUCTavg < foeUCTavg) return MoveReaction.NEGATIVE;
                    //might be amused depending on personality
                    return MoveReaction.CONFUSED;
                }
            } else {
                if (move.Equals(bestMove)) {
                    if (myUCTavg > foeUCTavg) {
                        return MoveReaction.NEUTRAL;
                    } else {
                        //depending on personality might be neutral
                        return MoveReaction.NEGATIVE;
                    }
                }

                if (move.Equals(worstMove)) {
                    if (myUCTavg < foeUCTavg) return MoveReaction.CONFUSED;
                    //might be amused depending on personality
                    return MoveReaction.POSITIVE;
                }
            }

            return MoveReaction.NEUTRAL;
        }

        [SerializeField]
        private float myUCTavg = 0;
        [SerializeField]
        private int its = 0;
        [SerializeField]
        private float foeUCTavg = 0;

        public void EvaluateConfidence(Networking.FeedData d, bool isMyTurn) {
            float myUCT, foeUCT, uctDiff, valence, arousal;
            float mySwing, foeSwing;
            valence = 0;
            arousal = 0;
            if(player == Player.First) {
                myUCT = d.FirstUCT;
                foeUCT = d.SecondUCT;
            } else {
                myUCT = d.SecondUCT;
                foeUCT = d.FirstUCT;
            }

            bestMove = d.Best;
            worstMove = d.Worst;
            forWho = isMyTurn ? player : (player == Player.First ? Player.Second : Player.First);

            uctDiff = myUCT - foeUCT;
            var last = myUCTavg;
            //need to observe how these change
            myUCTavg = myUCTavg * Config.UCTDecay + myUCT * (1 - Config.UCTDecay);
            //we need a stable average before we try to detect swings...
            if (its > 50) {
                arousal += Mathf.Abs(last - myUCTavg)/5;
            }

            //not sure this matters
            //if (isMyTurn) {
                //this is a promising move for me which I'm disproportionately simulating
                if (d.SimulationStdDev >= 2.5) {
                    if (myUCT > foeUCT) {
                        //Debug.Log("likes");
                        if (pm.GetArousal() < Config.Neutral) arousal += 15;
                        valence += 0.2f;
                        arousal += 0.3f;
                    } else { //my best move isn't even in my favour
                        //Debug.Log("no no no");
                        if (pm.GetArousal() < Config.Neutral) arousal += 15;
                        valence -= 0.2f;
                        arousal += 0.3f;
                    }
                } else if (d.SimulationStdDev < 2.5f && d.SimulationStdDev >= 2f) {
                    if (myUCT > foeUCT) {
                       // Debug.Log("thinks is pretty decent");
                        valence += 0.1f;
                        arousal += 0.1f;
                    } else { //my best move isn't even in my favour
                      //  Debug.Log("ehhhh");
                        valence -= 0.1f;
                        arousal += 0.12f;
                    }
                } else if (d.SimulationStdDev < 2f) { 
                    //moves are pretty even for me, in my favour, so I'm feeling calm
                    if (myUCT > foeUCT) {
                       // Debug.Log("chillax");
                        if (pm.GetArousal() > Config.Neutral) arousal -= 15;
                        arousal -= 0.2f;
                        valence += 0.1f;
                    } else {
                        //pretty uniformly bad for me eh
                      //  Debug.Log("sadness...");
                        if (pm.GetArousal() > Config.Neutral) arousal -= 15;
                        arousal -= 0.2f;
                        valence -= 0.1f;
                    }
                }
           // }

            //game unique weights here affecting the factors?
            //differences in how much confidence bounds vary between games
            valence += uctDiff * 0.05f;
        
            pm.Evaluate(valence, arousal);

            its++;

        }

        private void interpret(FMLBody body) {
            BMLBody last = null;
            foreach(var chunk in body.chunks) {
                BMLBody curr = new BMLBody();
                chunk.BMLRef = curr;                
                foreach (var function in chunk.functions) {
                    switch(function.Function) {
                        case FMLFunction.FunctionType.BOARDGAME_REACT_MOVE:
                            ReactMoveFunction react = function as ReactMoveFunction;
                            MoveReaction reaction = this.react(react.MoveToReact[0], react.MyMove ? player : (player == Player.First ? Player.Second : Player.First));
                            Debug.Log(reaction);
                            FaceEmotion faceReact;
                            switch (reaction) {
                                case MoveReaction.CONFUSED:
                                    faceReact = new FaceEmotion("ConfusedFace", chunk.owner, 0f, 1.8f, 0.6f);
                                    curr.AddChunk(faceReact);
                                    break;
                                case MoveReaction.NEGATIVE:
                                    faceReact = new FaceEmotion("NegativeFace", chunk.owner, 0f, 1.2f, 0.4f);
                                    curr.AddChunk(faceReact);
                                    break;
                                case MoveReaction.POSITIVE:
                                    faceReact = new FaceEmotion("HappyFace", chunk.owner, 0f, 1.4f, 2.5f);
                                    curr.AddChunk(faceReact);
                                    break;
                            }
                            break;
                        case FMLFunction.FunctionType.BOARDGAME_CONSIDER_MOVE:
                            ConsiderMoveFunction func = function as ConsiderMoveFunction;
                            if (func.MoveToConsider.Type == MoveType.MOVE) {
                                Gaze glanceFrom = new Gaze("glanceAtFrom", chunk.owner, BoardgameManager.Instance.GetCellObject(func.MoveToConsider.From), 
                                    Behaviour.Lexemes.Influence.HEAD, start: 0f, end: 2f);
                                curr.AddChunk(glanceFrom);
                                Gaze glanceTo = new Gaze("glanceAtTo", chunk.owner, BoardgameManager.Instance.GetCellObject(func.MoveToConsider.To), 
                                    Behaviour.Lexemes.Influence.HEAD, start: 2.1f, end: 2f);
                                curr.AddChunk(glanceTo);
                            } else {
                                Gaze glanceTo = new Gaze("glanceAtCell", chunk.owner, BoardgameManager.Instance.GetCellObject(func.MoveToConsider.To),
                                   Behaviour.Lexemes.Influence.HEAD, start: 0f, end: 2f);
                                curr.AddChunk(glanceTo);
                            }
                            
                            break;
                        case FMLFunction.FunctionType.BOARDGAME_MAKE_MOVE:
                            MakeMoveFunction move = function as MakeMoveFunction;
                            BoardgameManager.Instance.MoveMade(move.MoveToMake, player);
                            PhysicalCell from, to;
                            BoardgameManager.Instance.GetMoveFromTo(move.MoveToMake[0], player, out from, out to);
                            Grasp reach = new Grasp("reachTowardsPiece", chunk.owner, from.gameObject,
                                Behaviour.Lexemes.Mode.LEFT_HAND, (arm) => { GraspPiece(from, arm); }, 0, end: 1.5f);
                            Posture leanReach = new Posture("leantowardsPiece", chunk.owner, Behaviour.Lexemes.Stance.SITTING, 0, end: 1.5f);
                            Gaze lookReach = new Gaze("glanceAtReach", chunk.owner, from.gameObject, Behaviour.Lexemes.Influence.HEAD, start: 0f, end: 1.25f);
                            leanReach.AddPose(Behaviour.Lexemes.BodyPart.WHOLEBODY, Behaviour.Lexemes.BodyPose.LEANING_FORWARD);
                            Place place = new Place("placePiece", chunk.owner, to.gameObject,
                                Behaviour.Lexemes.Mode.LEFT_HAND, (piece) => { PlacePiece(piece, to); }, 1.25f, end: 2f);
                            Gaze lookPlace = new Gaze("glanceAtPlace", chunk.owner, to.gameObject, Behaviour.Lexemes.Influence.HEAD, start: 1.25f, end: 2f);
                            Gaze glance = new Gaze("glanceAtPlayer", chunk.owner, motion.Player, Behaviour.Lexemes.Influence.HEAD, start: 2f, end: 1.25f);
                            curr.AddChunk(glance);
                            curr.AddChunk(lookReach);
                            curr.AddChunk(lookPlace);
                            curr.AddChunk(leanReach);
                            curr.AddChunk(reach);
                            curr.AddChunk(place);
                            break;
                        case FMLFunction.FunctionType.EMOTION:
                            //transform expression
                            EmotionFunction f = function as EmotionFunction;
                            FaceEmotion fe = new FaceEmotion("emote " + f.Arousal + " " + f.Valence, chunk.owner, 0f, ((f.Arousal-Config.Neutral)*0.8f)+Config.Neutral, f.Valence);
                            //TODO: make this bml
                            float lean = Mathf.Clamp((f.Arousal - Config.Neutral) * 50, -20, 30);
                            
                            motion.SetLean(lean);
                            //Debug.Log(lean);
                            curr.AddChunk(fe);
                            break;
                    }
                }
            }
            //sort out timing
            foreach(var chunk in body.chunks) {
                if (chunk.timing != null) {
                    switch (chunk.timing.Primitive) {
                        case Primitive.MustEndBefore:
                            chunk.timing.ChunkReference.BMLRef.ExecuteAfter(chunk.BMLRef);
                            break;
                        case Primitive.StartImmediatelyAfter:
                            chunk.BMLRef.ExecuteAfter(chunk.timing.ChunkReference.BMLRef);
                            break;
                    }
                }
            }

            //let's refactor this later so that we don't have to do 3N
            foreach(var chunk in body.chunks) {
                behave.ScheduleBehaviour(chunk.BMLRef);
            }
        }

        public void GraspPiece(PhysicalCell piece, ActorMotion.Arm arm) {
            GameObject p = piece.RemovePiece();
            p.transform.SetParent(arm.transform);
            p.transform.localPosition = Vector3.zero;
            arm.holding = p.transform;
        }

        public void PlacePiece(GameObject piece, PhysicalCell newCell) {
            newCell.Place(piece);
        }

        float lastTime = -1f;
        public void ConsiderMove(Move move) {
            if (lastTime == -1) lastTime = Time.time;
            FMLBody body = new FMLBody();
            if (Time.time >= lastTime  + 4f) {                
                MentalChunk mc = new MentalChunk();
                //eye movement every 4
                mc.AddFunction(new ConsiderMoveFunction(move));
                mc.owner = me;
                body.AddChunk(mc);
                body.AddChunk(getEmotion());
                interpret(body);
                lastTime = Time.time;
            } else if(Time.time >= lastTime + 2f) {
                //emotions every second
                body.AddChunk(getEmotion());
                interpret(body);
            }
            
        }

        public void ExecuteMove(List<Move> moves) {
            FMLBody body = new FMLBody();
            PerformativeChunk pc = new PerformativeChunk();

            pc.AddFunction(new MakeMoveFunction(moves));
            pc.owner = me;
            body.AddChunk(pc);     
            body.AddChunk(getEmotion());

            interpret(body);
        }

        public void ReactMove(List<Move> moves, Player who) {
            FMLBody body = new FMLBody();
            PerformativeChunk pc = new PerformativeChunk();

            pc.AddFunction(new ReactMoveFunction(moves, who == player));
            pc.owner = me;
            body.AddChunk(pc);

            interpret(body);
        }
    }
}
