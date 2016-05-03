using System;
using System.Collections.Generic;
using UnityEngine;

namespace Boardgame.GDL {
    public class CheckersReader : GameReader {
        public CheckersReader() {
            lexer = new Lexer("cell", moveID: "move", controlID: "control", captureID: "capture");
        }

        public override State GetBoardState(string message) {
            State state;
            state.Control = Player.First;
            //Debug.Log(message);

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
                    Advance(ref token);
                    type += " " + token.Current.value;
                    list.Add(new Cell(id, type));
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

        protected override Move ParseSingleMove(ref List<Token>.Enumerator token) {
            string cellID, toID;
            switch (token.Current.type) {
                case TokenType.MOVE:
                    Advance(ref token);
                    cellID = token.Current.value;
                    Advance(ref token);
                    cellID += " " + token.Current.value;
                    Advance(ref token);
                    toID = token.Current.value;
                    Advance(ref token);
                    toID += " " + token.Current.value;
                    return new Move(cellID, toID);
                case TokenType.CAPTURE:
                    Advance(ref token);
                    cellID = token.Current.value;
                    Advance(ref token);
                    cellID += " " + token.Current.value;
                    Advance(ref token);
                    toID = token.Current.value;
                    Advance(ref token);
                    toID += " " + token.Current.value;
                    return new Move(MoveType.CAPTURE, cellID, toID);
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
