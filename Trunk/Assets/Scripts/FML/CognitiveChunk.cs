using System.Collections.Generic;

namespace Fml
{
    public class CognitiveChunk : FMLChunk
    {

        public TrackType trackType { get { return TrackType.MentalState; } }
        public Timing timing;
    }
}
