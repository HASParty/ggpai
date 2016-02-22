using System;
using System.Collections.Generic;
using UnityEngine;

namespace Boardgame.GDL {
    public class MyllaReader : GameReader {
        public MyllaReader() {
            lexer = new Lexer("cell", "place", "move", "remove", "heap");
        }

        public override State GetBoardState(string message) {
            State state;
            state.WhiteHandCount = 0;
            state.BlackHandCount = 0;

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
                            state.WhiteHandCount = count;
                            break;
                        case "black":
                            state.BlackHandCount = count;
                            break;
                    }
                }
            }

            state.Cells = list.ToArray();
            Debug.Log("Black: " + state.BlackHandCount + " White: " + state.WhiteHandCount);
            Debug.Log(Tools.Stringify<Cell>.Array(state.Cells));
            return state;
        }

        public override List<KeyValuePair<string, string>> GetMove(string message) {
            string lexify = Parser.BreakMessage(message).action;
            return ParseMoves(lexify);
        }

        private List<KeyValuePair<string, string>> ParseMoves(string moves) {
            var lexed = lexer.Lex(moves);
            var token = lexed.GetEnumerator();
            var list = new List<KeyValuePair<string, string>>();

            while (token.MoveNext()) {
                string cellID;
                switch (token.Current.type) {
                    case TokenType.MOVE:
                        string toID;
                        Advance(ref token);
                        cellID = token.Current.value;
                        Advance(ref token);
                        toID = token.Current.value;
                        list.Add(new KeyValuePair<string, string>(cellID, toID));
                        break;
                    case TokenType.PLACE:
                        Advance(ref token);
                        cellID = token.Current.value;
                        list.Add(new KeyValuePair<string, string>("PLACE", cellID));
                        break;
                    case TokenType.REMOVE:
                        Advance(ref token);
                        cellID = token.Current.value;
                        list.Add(new KeyValuePair<string, string>("REMOVE", cellID));
                        break;
                }
            }
            return list;
        }

        public override List<KeyValuePair<string, string>> GetLegalMoves(string message) {
            string lexify = Parser.BreakMessage(message).legalMoves;
            return ParseMoves(lexify);
        }
    }
}
