using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boardgame.GDL {
    public enum TokenType {
        LPAREN,
        RPAREN,
        TRUE,
        PIECE,
        CELL,
        PLACE,
        CAPTURE,
        MOVE,
        REMOVE,
        CONSTANT,
        HAND,
        CONTROL,
        CONSIDERED_MOVES,
        SIM_COUNT,
        UCT_VALUES,
        SSRATIO
    }

    public class Token {
        public TokenType type = TokenType.CONSTANT;
        public string value = "";

        public override string ToString() {
            if (value.Length > 0) {
                return String.Format("( Type: {0}, Value: {1} )", type.ToString(), value);
            } else {
                return String.Format("( Type: {0} )", type.ToString());
            }
        }
    }

    public class Lexer {
        string cellID;
        string placeID;
        string moveID;
        string removeID;
        string handID;
        string controlID;
        string captureID;

        public Lexer(string cellID, string placeID = "", string moveID = "", string removeID = "",
            string handID = "", string controlID = "", string captureID = "") {
            this.cellID = cellID;
            this.placeID = placeID;
            this.moveID = moveID;
            this.removeID = removeID;
            this.handID = handID;
            this.controlID = controlID;
            this.captureID = captureID;
        }

        public List<Token> Lex(string sentence) {
            List<Token> tok = new List<Token>();
            if (sentence == null) return tok;
            var split = sentence.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var w in split) {
                var word = w.Trim();
                Token token = new Token();
                if (word == "(") {
                    token.type = TokenType.LPAREN;
                } else if (word == ")") {
                    token.type = TokenType.RPAREN;
                } else if (word == cellID) {
                    token.type = TokenType.CELL;
                } else if (word == placeID) {
                    token.type = TokenType.PLACE;
                } else if (word == moveID) {
                    token.type = TokenType.MOVE;
                } else if (word == removeID) {
                    token.type = TokenType.REMOVE;
                } else if (word == captureID) {
                    token.type = TokenType.CAPTURE;
                } else if (word == "true") {
                    token.type = TokenType.TRUE;
                } else if (word == handID) {
                    token.type = TokenType.HAND;
                } else if (word == controlID) {
                    token.type = TokenType.CONTROL;
                } else if (word == "m:") {
                    token.type = TokenType.CONSIDERED_MOVES;
                } else if (word == "n:") {
                    token.type = TokenType.SIM_COUNT;
                } else if (word == "v:") {
                    token.type = TokenType.UCT_VALUES;
                } else {
                    token.value = word;
                }
                tok.Add(token);
            }

            return tok;
        }
    }
}
