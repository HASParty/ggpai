using System.Collections.Generic;

namespace FML {
    public class PerformativeChunk : FMLChunk {
        public override TrackType track { get { return TrackType.Performative; } }

        public PerformativeChunk() {
            functions = new List<FMLFunction>();
        }

        public override bool AddFunction(FMLFunction func) {
            functions.Add(func);
            return true;
        }
    }
}
