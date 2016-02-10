using System.Collections.Generic;

namespace Fml
{
    public class FMLChunk
    {
        public enum TrackType
        {
            Interactional,
            Performative,
            MentalState
        };

        public TrackType trackType;
        public Timing timing;
    }
}
