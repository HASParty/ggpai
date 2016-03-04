using System;
using System.Collections.Generic;
using UnityEngine;

namespace Boardgame.GDL {
    public class MyllaReader : GameReader {
        public MyllaReader() {
            lexer = new Lexer("pit", "place", "move", "remove", "heap", "control");
        }


        //TODO: refactor into parent class, make parsesinglemove something to be overridden
        //by children
        public override List<ConsideredMove> GetConsideredMoves(string message) {
            var cons = new List<ConsideredMove>();
            var token = lexer.Lex(Parser.CleanMessage(message)).GetEnumerator();
            while(token.MoveNext()) {
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

        public override State GetBoardState(string message) {
            State state;
            state.Control = Player.First;

            var list = new List<Cell>();
            string lexify = Parser.BreakMessage(message).state;
            var lexed = lexer.Lex(lexify);
            var token = lexed.GetEnumerator();

            while (token.MoveNext()) {
                if (token.Current.type == TokenType.CELL) {
                    string id, type;
                    Advance(ref token);
                    id = token.Current.value;
                    Advance(ref token);
                    id += " " + token.Current.value;
                    Advance(ref token);
                    type = token.Current.value;
                    list.Add(new Cell(id, type));
                } else if (token.Current.type == TokenType.HAND) {
                    string who;
                    int count;
                    Advance(ref token);
                    who = token.Current.value;
                    Advance(ref token);
                    if (!Int32.TryParse(token.Current.value, out count)) {
                        throw new Exception("INVALID GDL, expected int");
                    }
                    switch (who) {
                        case "white":
                            list.Add(new Cell("white heap", "white", count));
                            break;
                        case "black":
                            list.Add(new Cell("black heap", "black", count));
                            break;
                    }
                } else if (token.Current.type == TokenType.CONTROL) {
                    Advance(ref token);
                    switch (token.Current.value) {
                        case "black":
                            state.Control = Player.Second;
                            break;
                        case "white":
                            state.Control = Player.First;
                            break;
                    }
                }
            }

            state.Cells = list.ToArray();
            return state;
        }

        public override List<Move> GetMove(string message) {
            string lexify = Parser.BreakMessage(message).action;
            return ParseMoves(lexify);
        }

        private Move ParseSingleMove(ref List<Token>.Enumerator token) {
            string cellID;
            switch (token.Current.type) {
                case TokenType.MOVE:
                    string toID;
                    Advance(ref token);
                    cellID = token.Current.value;
                    Advance(ref token);
                    cellID += " " + token.Current.value;
                    Advance(ref token);
                    toID = token.Current.value;
                    Advance(ref token);
                    toID += " " + token.Current.value;
                    return new Move(cellID, toID);
                case TokenType.PLACE:
                    Advance(ref token);
                    cellID = token.Current.value;
                    Advance(ref token);
                    cellID += " " + token.Current.value;
                    return new Move(MoveType.PLACE, cellID);
                case TokenType.REMOVE:
                    Advance(ref token);
                    cellID = token.Current.value;
                    Advance(ref token);
                    cellID += " " + token.Current.value;
                    return new Move(MoveType.REMOVE, cellID);
            }

            return null;
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

        public override List<Move> GetLegalMoves(string message) {
            string lexify = Parser.BreakMessage(message).legalMoves;
            return ParseMoves(lexify);
        }
    }
}
