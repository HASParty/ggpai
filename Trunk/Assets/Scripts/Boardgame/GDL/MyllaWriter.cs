using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boardgame.GDL {
    public class MyllaWriter : GameWriter {
        public override string WriteMove(Move move) {
            return string.Format("( {0} {1} {2} )", move.Type.ToString().ToLower(), move.From, move.To);
        }
    }
}
