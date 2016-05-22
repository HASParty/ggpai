using System;
using System.Collections.Generic;

namespace Boardgame.GDL {
    public abstract class GameReader {
        protected Lexer lexer;

        protected void Advance(ref List<Token>.Enumerator tok, TokenType expect = TokenType.CONSTANT) {
            if (!tok.MoveNext() || tok.Current.type != expect) {
                throw new Exception("INVALID GDL: " + expect.ToString() + " expected, got " + tok.Current.ToString());
            }
        }

        public bool IsStart(string data) {
            return Parser.BreakMessage(data).action.Trim() == "ready";
        }

        public bool IsDone(string data) {
            return Parser.BreakMessage(data).action.Trim() == "done";
        }

        public bool IsBusy(string data) {
            return Parser.BreakMessage(data).action.Trim() == "busy";
        }

        public Terminal GetTerminal(string data) {
            var s = Parser.BreakMessage(data).terminal.Trim();
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

        public List<Move> GetMove(string message) {
            string lexify = Parser.BreakMessage(message).action;
            return ParseMoves(lexify);
        }

        private List<Move> ParseMoves(string moves) {
            var lexed = lexer.Lex(moves);
            var token = lexed.GetEnumerator();
            var list = new List<Move>();


            while (token.MoveNext()) {
                var move = ParseSingleMove(ref token);
                if (move != null) list.Add(move);
            }
            return list;
        }

        public List<Move> GetLegalMoves(string message) {
            string lexify = Parser.BreakMessage(message).legalMoves;
            return ParseMoves(lexify);
        }

        public abstract State GetBoardState(string message);
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
