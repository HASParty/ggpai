using Boardgame.GDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FML.Boardgame {
    public class ReactMoveFunction : FMLFunction {

        public override FunctionType Function { get { return FunctionType.BOARDGAME_REACT_MOVE; } }

        public List<Move> MoveToReact;
        public bool MyMove;

        public ReactMoveFunction(List<Move> move, bool isMyMove) {
            MoveToReact = move;
            MyMove = isMyMove;
        }

    }
}
