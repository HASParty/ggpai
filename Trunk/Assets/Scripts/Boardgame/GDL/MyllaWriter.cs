using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boardgame.GDL {
    public class MyllaWriter : GameWriter {
        public override string WriteMove(Move move) {
            if(move.Type == MoveType.MOVE) return string.Format("( move {0} {1} )", move.From, move.To);
            return string.Format("( {0} {1} )", move.Type.ToString().ToLower(), move.From);
        }
    }
}
