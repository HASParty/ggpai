using System.Collections.Generic;

namespace Fml
{
    public abstract class FMLFunction
    {
        public enum FunctionType
        {
            //mental state
            EMOTION,
            COGNITIVE_PROCESS,
            //interactional
            GROUNDING,
            TURN_TAKING,
            SPEECH_ACT,
            INITIATION,
            CLOSING,
            //performative
            //TODO: boardgame functions
        }

        public virtual FunctionType Function { get; protected set; }
    }
}
