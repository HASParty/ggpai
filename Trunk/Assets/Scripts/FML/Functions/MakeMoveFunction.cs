using Boardgame.GDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FML.Boardgame {
    public class MakeMoveFunction : FMLFunction {

        public override FunctionType Function { get { return FunctionType.BOARDGAME_MAKE_MOVE; } }

        public List<Move> MoveToMake;

        //something that dictates how sure he is about it or should that happen during the mapping process?

        public MakeMoveFunction(List<Move> move) {
            MoveToMake = move;
        }

    }
}
