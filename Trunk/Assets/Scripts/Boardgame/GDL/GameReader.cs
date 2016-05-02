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
        DRAW,
        DONE
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
                throw new Exception("INVALID GDL: "+expect.ToString()+" expected, got "+tok.Current.ToString());
            }
        }

        public bool IsStart(string data) {
            return Parser.BreakMessage(data).action.Trim() == "ready";
        }

        public bool IsDone(string data)
        {
            return Parser.BreakMessage(data).action.Trim() == "done";
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
        protected abstract Move ParseSingleMove(ref List<Token>.Enumerator token);
        public List<ConsideredMove> GetConsideredMoves(string message) {
            var cons = new List<ConsideredMove>();
            var token = lexer.Lex(Parser.CleanMessage(message)).GetEnumerator();
            while (token.MoveNext()) {
                if (token.Current.type == TokenType.LPAREN) {
                    int parenOpen = 1;
                    while (parenOpen > 0) {
                        ConsideredMove move = new ConsideredMove();
                        token.MoveNext();
                        if (token.Current.type == TokenType.LPAREN) {
                            parenOpen++;
                            while (parenOpen > 1 && token.MoveNext()) {
                                switch (token.Current.type) {
                                    case TokenType.CONSIDERED_MOVES:
                                        token.MoveNext();
                                        bool open = false;
                                        if (token.Current.type == TokenType.LPAREN) {
                                            open = true;
                                            token.MoveNext();
                                        }
                                        move.First = ParseSingleMove(ref token);
                                        if (open) {
                                            open = false;
                                            Advance(ref token, TokenType.RPAREN);
                                        }
                                        token.MoveNext();
                                        if (token.Current.type == TokenType.LPAREN) {
                                            open = true;
                                            token.MoveNext();
                                        }
                                        move.Second = ParseSingleMove(ref token);
                                        if (open) {
                                            open = false;
                                            Advance(ref token, TokenType.RPAREN);
                                        }
                                        break;
                                    case TokenType.SIM_COUNT:
                                        Advance(ref token);
                                        if (!Int32.TryParse(token.Current.value, out move.Simulations)) {
                                            throw new Exception("INVALID GDL, expected int");
                                        }
                                        break;
                                    case TokenType.UCT_VALUES:
                                        Advance(ref token);
                                        if (!float.TryParse(token.Current.value.Replace("f", ""), out move.FirstUCT)) {
                                            throw new Exception("INVALID GDL, expected float");
                                        }
                                        Advance(ref token);
                                        if (!float.TryParse(token.Current.value.Replace("f", ""), out move.SecondUCT)) {
                                            throw new Exception("INVALID GDL, expected float");
                                        }
                                        break;
                                    case TokenType.RPAREN:
                                        parenOpen--;
                                        break;
                                    case TokenType.LPAREN:
                                        parenOpen++;
                                        break;
                                }
                            }
                            cons.Add(move);
                        } else if (token.Current.type == TokenType.RPAREN) {
                            parenOpen--;
                        }
                    }
                }
            }

            return cons;
        }
    }
}
