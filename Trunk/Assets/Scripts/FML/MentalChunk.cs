using System.Collections.Generic;

namespace FML {
    public class MentalChunk : FMLChunk {
        public TrackType TrackType { get { return TrackType.MentalState; } }

        public bool AddFunction(FMLFunction func) {
            functions.Add(func);
            return true;
        }

        public MentalChunk() {
            functions = new List<FMLFunction>();
        }
    }
}
