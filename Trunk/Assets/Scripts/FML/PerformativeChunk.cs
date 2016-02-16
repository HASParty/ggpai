using System.Collections.Generic;

namespace Fml
{
    public class PerformativeChunk : FMLChunk
    {
        public TrackType TrackType { get { return TrackType.Performative; } }

        public PerformativeChunk()
        {
            functions = new List<FMLFunction>();
        }
    }
}
