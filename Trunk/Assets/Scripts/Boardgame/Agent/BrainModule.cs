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
    /// <summary>
    /// A script that attempts to make sense of the game state and uses the agent's
    /// personality to determine his reaction to it, and interprets FML chunks appropriately.
    /// Essentially the agent's brain...
    /// </summary>
    [RequireComponent(typeof(PersonalityModule), typeof(InputModule), typeof(BehaviourRealiser))]
    public class BrainModule : MonoBehaviour {
        private PersonalityModule pm;
        private InputModule im;
        private BehaviourRealiser behave;
        private ActorMotion motion;
        private Participant me;

        /// <summary>
        /// Which player the agent is in the current game.
        /// </summary>
        public Player player;

        void Awake() {
            pm = GetComponent<PersonalityModule>();
            im = GetComponent<InputModule>();
            behave = transform.parent.GetComponentInChildren<BehaviourRealiser>();
            me = new Participant();
            me.identikit = GetComponent<Identikit>();
            motion = transform.parent.GetComponentInChildren<ActorMotion>();
        }


        #region React to move
        private Move bestMove;
        private Move worstMove;
        private Player forWho; //making sure the data is in sync
        private enum MoveReaction {
            CONFUSED,
            POSITIVE,
            NEGATIVE,
            NEUTRAL
        }
        
        /// <summary>
        /// Using the data currently stored, attempt to work out whether the
        /// move was a "surprise" and such for the AI
        /// </summary>
        /// <param name="move">The move in question</param>
        /// <param name="who">The player who made it</param>
        /// <returns>The agent's reaction</returns>
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
                else {

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
        #endregion

        #region React to general game state
        private float myUCTavg = 0;
        private int its = 0;
        private float foeUCTavg = 0;

        /// <summary>
        /// Evaluates the state of the game via data fed by the GGP AI.
        /// Adjusts mood accordingly.
        /// </summary>
        /// <param name="d">The data</param>
        /// <param name="isMyTurn">whether it is the agent's turn or not</param>
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
            myUCTavg = myUCTavg * Config.UCTDecay + myUCT * (1 - Config.UCTDecay);
            foeUCTavg = foeUCTavg * Config.UCTDecay + foeUCT * (1 - Config.UCTDecay);
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

        #endregion

        #region Transform FML into BML and helpers
        /// <summary>
        /// Transforms FML into BML and executes
        /// </summary>
        /// <param name="body">the FML body containing the FML functions to be interpreted</param>
        private void interpret(FMLBody body) {
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
                            Posture poser = new Posture("postureReact", chunk.owner, Behaviour.Lexemes.Stance.SITTING, 0f, 8f);
                            switch (reaction) {
                                case MoveReaction.CONFUSED:
                                    faceReact = new FaceEmotion("ConfusedFace", chunk.owner, 0f, 1.8f*pm.GetArousalConfusedMod(), 0.6f*pm.GetValenceConfusedMod());
                                    poser.AddPose(Behaviour.Lexemes.BodyPart.RIGHT_ARM, Behaviour.Lexemes.BodyPose.FIST_COVER_MOUTH);
                                    curr.AddChunk(faceReact);
                                    curr.AddChunk(poser);
                                    break;
                                case MoveReaction.NEGATIVE:
                                    faceReact = new FaceEmotion("NegativeFace", chunk.owner, 0f, 1.2f * pm.GetArousalNegativeMod(), 0.4f * pm.GetValenceNegativeMod());
                                    poser.AddPose(Behaviour.Lexemes.BodyPart.ARMS, Behaviour.Lexemes.BodyPose.ARMS_CROSSED);
                                    curr.AddChunk(faceReact);
                                    curr.AddChunk(poser);
                                    break;
                                case MoveReaction.POSITIVE:
                                    faceReact = new FaceEmotion("HappyFace", chunk.owner, 0f, 1.4f * pm.GetArousalPositiveMod(), 2.5f * pm.GetValencePositiveMod());
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
                                Behaviour.Lexemes.Mode.LEFT_HAND, (arm) => { graspPiece(from, arm); }, 0, end: 1.5f);
                            Posture leanReach = new Posture("leantowardsPiece", chunk.owner, Behaviour.Lexemes.Stance.SITTING, 0, end: 1.5f);
                            Gaze lookReach = new Gaze("glanceAtReach", chunk.owner, from.gameObject, Behaviour.Lexemes.Influence.HEAD, start: 0f, end: 1.25f);
                            Debug.Log(Vector3.Distance(from.transform.position, transform.position));
                            leanReach.AddPose(Behaviour.Lexemes.BodyPart.WHOLEBODY, Behaviour.Lexemes.BodyPose.LEANING_FORWARD, 
                                (int)(Vector3.Distance(from.transform.position, transform.position)*10));
                            Place place = new Place("placePiece", chunk.owner, to.gameObject,
                                Behaviour.Lexemes.Mode.LEFT_HAND, (piece) => { placePiece(piece, to); }, 1.25f, end: 2f);
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
                            float lean = Mathf.Clamp((f.Arousal - Config.Neutral) * 50, -20, 30);
                            Posture emoLean = new Posture("emoteLean", chunk.owner, Behaviour.Lexemes.Stance.SITTING, 0, end: 2f);
                            emoLean.AddPose(Behaviour.Lexemes.BodyPart.WHOLEBODY, Behaviour.Lexemes.BodyPose.LEANING_FORWARD, (int)lean);
                            curr.AddChunk(emoLean);
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

        /// <summary>
        /// Callback for grasping pieces, called upon reaching target
        /// </summary>
        /// <param name="cell">Cell from where to get piece</param>
        /// <param name="arm">The arm that should take the piece</param>
        private void graspPiece(PhysicalCell cell, ActorMotion.Arm arm) {
            GameObject p = cell.RemovePiece();
            p.transform.SetParent(arm.transform);
            p.transform.localPosition = Vector3.zero;
            arm.holding = p.transform;
        }

        /// <summary>
        /// Callback for placing pieces, called upon reaching cell
        /// </summary>
        /// <param name="piece">The piece gameObject</param>
        /// <param name="newCell">Cell in which to place piece</param>
        private void placePiece(GameObject piece, PhysicalCell newCell) {
            newCell.Place(piece);
        }
        #endregion

        #region FML generation
        float lastTime = -1f;
        /// <summary>
        /// Generates the appropriate FML for considering the given move.
        /// Every 2 seconds, generates the emotion only.
        /// Every 4 seconds, generates both emotion and move consideration.
        /// Interprets it instantly.
        /// </summary>
        /// <param name="move">Move to consider.</param>
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

        /// <summary>
        /// Generates the FML for executing a move and interprets it.
        /// </summary>
        /// <param name="moves"></param>
        public void ExecuteMove(List<Move> moves) {
            FMLBody body = new FMLBody();
            PerformativeChunk pc = new PerformativeChunk();

            pc.AddFunction(new MakeMoveFunction(moves));
            pc.owner = me;
            body.AddChunk(pc);     
            body.AddChunk(getEmotion());

            interpret(body);
        }

        /// <summary>
        /// Generate a reaction to a move
        /// </summary>
        /// <param name="moves">Moves</param>
        /// <param name="who">who made the move(s)</param>
        public void ReactMove(List<Move> moves, Player who) {
            FMLBody body = new FMLBody();
            PerformativeChunk pc = new PerformativeChunk();

            pc.AddFunction(new ReactMoveFunction(moves, who == player));
            pc.owner = me;
            body.AddChunk(pc);

            interpret(body);
        }

        /// <summary>
        /// Helper that generates an emotion function.
        /// </summary>
        /// <returns>emotion function with data from current mood</returns>
        private MentalChunk getEmotion() {
            MentalChunk chunk = new MentalChunk();
            chunk.AddFunction(new EmotionFunction(pm.GetArousal(), pm.GetValence()));
            chunk.owner = me;
            return chunk;
        }
        #endregion
    }
}
