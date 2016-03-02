using Behaviour;
using System.Collections.Generic;

namespace FML {
    public class FMLBody {
        public List<FMLChunk> chunks { get; protected set; }

        public FMLBody() {
            chunks = new List<FMLChunk>();
        }

        public void AddChunk(FMLChunk chunk) {
            //maybe sorting?
            chunks.Add(chunk);
        }
    }
}
