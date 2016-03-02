using System.Collections.Generic;

namespace FML {
    public class MentalChunk : FMLChunk {
        public TrackType TrackType { get { return TrackType.MentalState; } }

        public bool AddFunction(FMLFunction func) {
            if (func.Function == FMLFunction.FunctionType.COGNITIVE_PROCESS ||
                func.Function == FMLFunction.FunctionType.EMOTION ||
                func.Function == FMLFunction.FunctionType.BOARDGAME_CONSIDER_MOVE) {
                functions.Add(func);
                return true;
            }
            return false;
        }

        public MentalChunk() {
            functions = new List<FMLFunction>();
        }
    }
}
