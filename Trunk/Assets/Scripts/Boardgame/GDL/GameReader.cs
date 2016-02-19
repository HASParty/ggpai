using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boardgame.GDL {
    public abstract class GameReader {
        protected Lexer lexer;

        public struct State {
            public Cell[] Cells;
            public int WhiteHandCount;
            public int BlackHandCount;
        }

        public class Cell {
            public Cell(string id, string type) {
                ID = id;
                Type = type;
            }
            public string ID;
            public string Type;

            public override string ToString() {
                return String.Format("( Cell id: {0}, type: {1} )", ID, Type);
            }
        }

        public abstract State GetBoardState(string message);
        public abstract List<KeyValuePair<string, string>> GetMove(string message);
        public abstract List<KeyValuePair<string, string>> GetLegalMoves(string message);
    }
}
