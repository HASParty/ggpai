using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boardgame.GDL {
    public enum MoveType {
        MOVE,
        PLACE,
        REMOVE
    }

    public class Move {
        public Move(string from, string to) {
            Type = MoveType.MOVE;
            From = from;
            To = to;
        }

        public Move(MoveType type, string at) {
            Type = type;
            From = at;
            To = at;
        }

        public MoveType Type {
            get;
            protected set;
        }

        public string From {
            get; protected set;
        }
        public string To {
            get; protected set;
        }

        public override string ToString() {
            if(Type != MoveType.MOVE) {
                return string.Format("( {0}: {1} )", Type, From);
            }
            return string.Format("( {0}: {1} -> {2} )", Type, From, To);
        }
    }
}
