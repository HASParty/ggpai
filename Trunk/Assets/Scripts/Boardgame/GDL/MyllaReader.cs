using System;
using System.Collections.Generic;
using UnityEngine;

namespace Boardgame.GDL {
    public class MyllaReader : GameReader {
        public MyllaReader() {
            lexer = new Lexer("cell", "place", "move", "remove", "heap", "control");
        }

        public override State GetBoardState(string message) {
            State state;
            state.Control = Player.Black;

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
                        case "white":
                            state.Control = Player.White;
                            break;
                        case "black":
                            state.Control = Player.Black;
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

        private List<Move> ParseMoves(string moves) {
            var lexed = lexer.Lex(moves);
            var token = lexed.GetEnumerator();
            var list = new List<Move>();

            while (token.MoveNext()) {
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
                        list.Add(new Move(cellID, toID));
                        break;
                    case TokenType.PLACE:
                        Advance(ref token);
                        cellID = token.Current.value;
                        Advance(ref token);
                        cellID += " " + token.Current.value;
                        list.Add(new Move(MoveType.PLACE, cellID));
                        break;
                    case TokenType.REMOVE:
                        Advance(ref token);
                        cellID = token.Current.value;
                        Advance(ref token);
                        cellID += " " + token.Current.value;
                        list.Add(new Move(MoveType.REMOVE, cellID));
                        break;
                }
            }
            return list;
        }

        public override List<Move> GetLegalMoves(string message) {
            string lexify = Parser.BreakMessage(message).legalMoves;
            return ParseMoves(lexify);
        }
    }
}
