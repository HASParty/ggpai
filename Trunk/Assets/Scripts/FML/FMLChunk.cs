using Behaviour;
using System.Collections.Generic;

namespace FML {
    public abstract class FMLChunk {
        public enum TrackType {
            Interactional,
            Performative,
            MentalState
        };

        public virtual TrackType track { get; protected set; }
        public Timing timing;

        public BMLBody BMLRef;

        public Participant owner;

        public virtual List<FMLFunction> functions { get; protected set; }

        public virtual bool AddFunction(FMLFunction func) {
            return false;
        }
    }
}
