using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boardgame.GDL {
    public class CheckersWriter : GameWriter {
        public override string WriteMove(Move move) {
            if (move.Type == MoveType.MOVE || move.Type == MoveType.CAPTURE) return string.Format("( ( {0} {1} {2} ) )", move.Type.ToString().ToLower(), move.From, move.To);
            return string.Format("( ( {0} {1} ) )", move.Type.ToString().ToLower(), move.From);
        }
    }
}
