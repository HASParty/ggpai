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

    public enum Terminal {
        FALSE,
        WIN,
        LOSS,
        DRAW
    }

    public class ConsideredMove {
        public Move First = null;
        public Move Second = null;
        public float FirstUCT = 0;
        public float SecondUCT = 0;
        public int Simulations = 0;

        public override string ToString() {
            return String.Format("( FIRST: {0} [{1}], SECOND: {2} [{3}], SIMULATIONS: {4} )",
                First, FirstUCT, Second, SecondUCT, Simulations);
        }
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

        public bool IsBusy(string data) {
            return Parser.BreakMessage(data).action.Trim() == "busy";
        }

        public Terminal GetTerminal(string data) {
            var s = Parser.BreakMessage(data).action.Trim();
            switch (s) {
                case "lost":
                    return Terminal.LOSS;
                case "won":
                    return Terminal.WIN;
                case "draw":
                    return Terminal.DRAW;
                default:
                    return Terminal.FALSE;
            }
        }

        public abstract State GetBoardState(string message);
        public abstract List<Move> GetMove(string message);
        public abstract List<Move> GetLegalMoves(string message);
        public abstract List<ConsideredMove> GetConsideredMoves(string message);
    }
}
