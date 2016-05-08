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
        private Mood mood;
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
            mood = GetComponent<Mood>();
        }


        #region React to move
        private Move myBestMove;
        private Dictionary<Move, Networking.FeedData.FMove> firstMoves;
        private Dictionary<Move, Networking.FeedData.FMove> secondMoves;
        private float firstAverageSims, secondAverageSims;
        private bool surprised = false;
        /// <summary>
        /// Using the data currently stored, attempt to work out whether the
        /// move was a "surprise" and such for the AI
        /// </summary>
        /// <param name="move">The move in question</param>
        /// <param name="who">The player who made it</param>
        /// <returns>The agent's reaction</returns>
        private void react(Move move, Player who) {
            float averageSims = (who == Player.First ? firstAverageSims : secondAverageSims);
            Debug.LogFormat("actual {0} avg {1}", who.ToString(), averageSims);
            var moves = (who == Player.First ? firstMoves : secondMoves);
            if (moves == null) return;
            Networking.FeedData.FMove m;
            try {
                m = moves[move];
            } catch (Exception e) { 
                Debug.LogWarning("Sync issue");
                Debug.Log(move);
                var ms = new Move[moves.Count];
                moves.Keys.CopyTo(ms, 0);
                Debug.Log(Tools.Stringify<Move>.Array(ms));
                return;
            }
            Debug.Log(m.Who);
            if (m.Who != who) return;
            Debug.LogFormat("player {0} sims {1} avg {2}", who.ToString(), m.Simulations, averageSims);
            Debug.LogFormat("Best {0} vs Actual {1}", myBestMove, move);
            if ((m.Simulations < averageSims && who != player) || (who == player && !move.Equals(myBestMove)))
            {
                Debug.Log("SURPRISED");
                surprised = true;
            }
            
        }

        private bool AckSurprise() {
            if(surprised) {
                surprised = false;
                return true;
            }
            return false;
        }
        #endregion

        #region React to general game state

        public float confidence = 0;
        private int confidentCount = 0;
        private int notConfidentCount = 0;
        private int disproportionateFavour = 0;
        private int opponentDisproportionateFavour = 0;
        private int weightedUCTOverFoe = 0;
        private int weightedUCTUnderFoe = 0;
        private int highSimCount = 0;
        /// <summary>
        /// Evaluates the state of the game via data fed by the GGP AI.
        /// Adjusts mood accordingly.
        /// </summary>
        /// <param name="d">The data</param>
        /// <param name="isMyTurn">whether it is the agent's turn or not</param>
        public void EvaluateConfidence(Networking.FeedData d, bool isMyTurn) {
            if (d.Best == null) return;
            if(isMyTurn) {               
                myBestMove = d.Best;
                if (player == Player.First) {
                    firstMoves = d.Moves;
                    firstAverageSims = (float)d.AverageSimulations;
                } else {
                    secondMoves = d.Moves;
                    secondAverageSims = (float)d.AverageSimulations;
                }
            } else {
                if (player == Player.First) {
                    secondMoves = d.Moves;
                    secondAverageSims = (float)d.AverageSimulations;
                } else {
                    firstMoves = d.Moves;
                    firstAverageSims = (float)d.AverageSimulations;
                }
            }
            float myUCT, foeUCT, myWUCT, foeWUCT, valence = 0, arousal = 0;
            float previousConfidence = confidence;
            valence = 0;
            arousal = 0;
            if(player == Player.First) {
                myUCT = d.Moves[d.Best].FirstUCT;
                foeUCT = d.Moves[d.Best].SecondUCT;
                myWUCT = d.FirstWeightedUCT;
                foeWUCT = d.SecondWeightedUCT;
            } else {
                myUCT = d.Moves[d.Best].SecondUCT;
                foeUCT = d.Moves[d.Best].FirstUCT;
                myWUCT = d.SecondWeightedUCT;
                foeWUCT = d.FirstWeightedUCT;
            }

            //increment counters for various states
            //best move is by far most simulated and is advantageous for me
            count(d.SimulationStdDev > 2.5f && myUCT > foeUCT, ref disproportionateFavour);
            count(d.SimulationStdDev > 2.5f && myUCT < foeUCT, ref opponentDisproportionateFavour);
            //my total weighted uct is greater than my foe's
            count(myWUCT > foeWUCT, ref weightedUCTOverFoe);
            count(myWUCT < foeWUCT, ref weightedUCTUnderFoe);
            //sim count high
            count(Config.SimulationCutoff < d.TotalSimulations / 2, ref highSimCount);

            //only start affecting confidence if these have been true for a few iterations
            //and stop affecting confidence when it has been true for a generous while
            if (stabilised(disproportionateFavour)) confidence += 1f*weight(disproportionateFavour);
            if (stabilised(weightedUCTOverFoe)) confidence += 1f*weight(weightedUCTOverFoe);
            if (stabilised(weightedUCTUnderFoe)) confidence -= 1f*weight(weightedUCTUnderFoe);
            if (stabilised(opponentDisproportionateFavour)) confidence -= 1f*weight(opponentDisproportionateFavour);
            if (stabilised(highSimCount)) confidence += 1f*weight(highSimCount);

            count(confidence > previousConfidence, ref confidentCount);
            count(confidence < previousConfidence, ref notConfidentCount);


            float diff = Mathf.Abs(confidence - previousConfidence);
            if(diff > 0.7f) {
                arousal += 1f;
            }

            if(stabilised(highSimCount)) {
                arousal -= 2f*weight(highSimCount);
            }

            if (stabilised(notConfidentCount))
            {
                //Debug.Log("DROP");
                valence -= 3f*weight(notConfidentCount);
                arousal += 2f*weight(notConfidentCount);
            }
            else if(stabilised(confidentCount))
            {
                //Debug.Log("INCR");
                valence += 3f*weight(confidentCount);
                arousal -= 2f*weight(confidentCount);
            }

            mood.Evaluate(valence, arousal);

        }

        private float weight(int val) {
            return (float)(100-val) / 100f;
        }

        private bool stabilised(int val)
        {
            return val > 5 && val < 100;
        }
        private void count(bool truthy, ref int val) {
            if (val > 500) {
                val = 0;
                return;
            }
            if (truthy) val = val+1;
            else val = 0;
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
                            this.react(react.MoveToReact[0], react.MyMove ? player : (player == Player.First ? Player.Second : Player.First));
                            FaceEmotion faceReact;
                            Posture poser = new Posture("postureReact", chunk.owner, Behaviour.Lexemes.Stance.SITTING, 0f, 8f);
                            if (AckSurprise())
                            {
                                faceReact = new FaceEmotion("ConfusedFace", chunk.owner, 0f, 1.6f, 0.6f);
                                poser.AddPose(Behaviour.Lexemes.BodyPart.RIGHT_ARM, Behaviour.Lexemes.BodyPose.FIST_COVER_MOUTH);
                                curr.AddChunk(faceReact);
                                curr.AddChunk(poser);
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
                            //Debug.Log(Tools.Stringify<Move>.List(move.MoveToMake));
                            
                            PhysicalCell from, to;
                            BoardgameManager.Instance.GetMoveFromTo(move.MoveToMake[0], player, out from, out to);
                            Grasp reach = new Grasp("reachTowardsPiece", chunk.owner, from.gameObject,
                                Behaviour.Lexemes.Mode.LEFT_HAND, (arm) => { graspPiece(from, arm); }, 0, end: 1.5f);
                            Posture leanReach = new Posture("leantowardsPiece", chunk.owner, Behaviour.Lexemes.Stance.SITTING, 0, end: 1.5f);
                            Gaze lookReach = new Gaze("glanceAtReach", chunk.owner, from.gameObject, Behaviour.Lexemes.Influence.HEAD, start: 0f, end: 1.25f);
                            //Debug.Log(Vector3.Distance(from.transform.position, transform.position));
                            leanReach.AddPose(Behaviour.Lexemes.BodyPart.WHOLEBODY, Behaviour.Lexemes.BodyPose.LEANING_FORWARD, 
                                (int)(Vector3.Distance(from.transform.position, transform.position)*20));
                            Place place = new Place("placePiece", chunk.owner, to.gameObject,
                                Behaviour.Lexemes.Mode.LEFT_HAND, (piece) => { placePiece(piece, to);
                                    BoardgameManager.Instance.MoveMade(move.MoveToMake, player); BoardgameManager.Instance.SyncState(); BoardgameManager.Instance.MakeNoise(to.id); }, 
                                1.25f, end: 2f);
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
                            Posture emoLean = new Posture("emoteLean", chunk.owner, Behaviour.Lexemes.Stance.SITTING, 0, end: 3f);
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
            chunk.AddFunction(new EmotionFunction(mood.GetArousal(), mood.GetValence()));
            chunk.owner = me;
            return chunk;
        }
        #endregion
    }
}
