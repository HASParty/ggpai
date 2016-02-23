using System;
using System.Collections.Generic;

namespace Boardgame.GDL {
    public class Cell {
        public Cell(string id, string type, int count = 0) {
            ID = id;
            Type = type;
            Count = count;
        }
        public string ID;
        public string Type;
        public int Count;

        public override string ToString() {
            return String.Format("( Cell id: {0}, type: {1}, count: {2} )", ID, Type, Count);
        }
    }

    public struct State {
        public Cell[] Cells;
        public Player Control;
    }

    public abstract class GameReader {
        protected Lexer lexer;      

        protected void Advance(ref List<Token>.Enumerator tok, TokenType expect = TokenType.CONSTANT) {
            if (!tok.MoveNext() || tok.Current.type != expect) {
                throw new Exception("INVALID GDL");
            }
        }

        public bool IsStart(string data) {
            return Parser.BreakMessage(data).action.Trim() == "ready";
        }

        public abstract State GetBoardState(string message);
        public abstract List<Move> GetMove(string message);
        public abstract List<Move> GetLegalMoves(string message);
    }
}
