using System.Collections.Generic;

namespace FML {
    public abstract class FMLFunction {
        public enum FunctionType {
            //mental state
            EMOTION,
            COGNITIVE_PROCESS,
            BOARDGAME_CONSIDER_MOVE, //?
            //interactional
            GROUNDING,
            TURN_TAKING,
            SPEECH_ACT,
            INITIATION,
            CLOSING,
            //performative
            BOARDGAME_MAKE_MOVE
        }

        public virtual FunctionType Function { get; protected set; }
    }
}
