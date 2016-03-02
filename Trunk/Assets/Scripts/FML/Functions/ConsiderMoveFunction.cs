using Boardgame.GDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FML.Boardgame {
    public class ConsiderMoveFunction : FMLFunction {

        public FunctionType Function { get { return FunctionType.BOARDGAME_CONSIDER_MOVE; } }

        public Move MoveToConsider;

        public ConsiderMoveFunction(Move move) {
            MoveToConsider = move;
        }

    }
}
