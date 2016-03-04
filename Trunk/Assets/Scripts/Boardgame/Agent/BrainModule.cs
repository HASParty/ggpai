using UnityEngine;
using System.Collections.Generic;
using Boardgame.GDL;
using FML;
using FML.Boardgame;
using Behaviour;

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
        }

        void Update() {
            //add in hacky idle glance at player?
        }

        private MentalChunk getEmotion() {
            MentalChunk chunk = new MentalChunk();
            chunk.AddFunction(new EmotionFunction(pm.GetArousal(), pm.GetValence()));
            chunk.owner = me;
            return chunk;
        }

        private void interpret(FMLBody body) {
            BMLBody last = null;
            foreach(var chunk in body.chunks) {
                BMLBody curr = new BMLBody();
                chunk.BMLRef = curr;                
                foreach (var function in chunk.functions) {
                    switch(function.Function) {
                        case FMLFunction.FunctionType.BOARDGAME_CONSIDER_MOVE:
                            ConsiderMoveFunction func = function as ConsiderMoveFunction;
                            if (func.MoveToConsider.Type == MoveType.MOVE) {
                                Gaze glanceFrom = new Gaze("glanceAtFrom", chunk.owner, BoardgameManager.Instance.GetCellObject(func.MoveToConsider.From), 
                                    Behaviour.Lexemes.Influence.HEAD, start: 0f, end: 1.75f);
                                curr.AddChunk(glanceFrom);
                                Gaze glanceTo = new Gaze("glanceAtTo", chunk.owner, BoardgameManager.Instance.GetCellObject(func.MoveToConsider.To), 
                                    Behaviour.Lexemes.Influence.HEAD, start: 1.8f, end: 2.75f);
                                curr.AddChunk(glanceTo);
                            } else {
                                Gaze glanceTo = new Gaze("glanceAtCell", chunk.owner, BoardgameManager.Instance.GetCellObject(func.MoveToConsider.To),
                                   Behaviour.Lexemes.Influence.HEAD, start: 0f, end: 2.75f);
                                curr.AddChunk(glanceTo);
                            }
                            
                            break;
                        case FMLFunction.FunctionType.BOARDGAME_MAKE_MOVE:
                            //make move, glance at player
                            //TODO: BMLIFY
                            BoardgameManager.Instance.MakeMove((function as MakeMoveFunction).MoveToMake, player);
                            Gaze glance = new Gaze("glanceAtPlayer", chunk.owner, motion.Player, Behaviour.Lexemes.Influence.HEAD, start: 0.25f, end: 0.5f);
                            curr.AddChunk(glance);
                            break;
                        case FMLFunction.FunctionType.EMOTION:
                            //transform expression
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

        float lastTime = -1f;
        public void ConsiderMove(Move move) {
            if (lastTime == -1) lastTime = Time.time;
            if(Time.time >= lastTime  + 4f) {
                FMLBody body = new FMLBody();
                MentalChunk mc = new MentalChunk();

                mc.AddFunction(new ConsiderMoveFunction(move));
                mc.owner = me;
                body.AddChunk(mc);
                body.AddChunk(getEmotion());

                interpret(body);
                lastTime = Time.time;
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
    }
}
