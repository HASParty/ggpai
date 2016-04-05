using System.Collections.Generic;

namespace FML {
    public class InteractionalChunk : FMLChunk {
        public override TrackType track { get { return TrackType.Interactional; } }

        public override bool AddFunction(FMLFunction func) {
            //maybe give functions track types...?
            if (func.Function == FMLFunction.FunctionType.GROUNDING ||
               func.Function == FMLFunction.FunctionType.CLOSING ||
               func.Function == FMLFunction.FunctionType.INITIATION ||
               func.Function == FMLFunction.FunctionType.SPEECH_ACT ||
               func.Function == FMLFunction.FunctionType.TURN_TAKING) {
                functions.Add(func);
                return true;
            }
            return false;
        }

        public InteractionalChunk() {
            functions = new List<FMLFunction>();
        }
    }
}
