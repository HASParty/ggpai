using System.Collections.Generic;

namespace FML {
    public class MentalChunk : FMLChunk {
        public override TrackType track { get { return TrackType.MentalState; } }

        public override bool AddFunction(FMLFunction func) {
            functions.Add(func);
            return true;
        }

        public MentalChunk() {
            functions = new List<FMLFunction>();
        }
    }
}
