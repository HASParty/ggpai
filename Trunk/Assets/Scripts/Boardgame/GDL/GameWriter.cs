using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boardgame.GDL {
    public abstract class GameWriter {
        public abstract string WriteMove(Move move);
    }
}
