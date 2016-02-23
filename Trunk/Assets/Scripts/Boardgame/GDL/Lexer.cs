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
        MOVE,
        REMOVE,
        CONSTANT,
        HAND,
        CONTROL
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

        public Lexer(string cellID, string placeID, string moveID = "", string removeID = "",
            string handID = "", string controlID = "") {
            this.cellID = cellID;
            this.placeID = placeID;
            this.moveID = moveID;
            this.removeID = removeID;
            this.handID = handID;
        }

        public List<Token> Lex(string sentence) {
            List<Token> tok = new List<Token>();
            var split = sentence.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var word in split) {
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
                } else if (word == "true") {
                    token.type = TokenType.TRUE;
                } else if (word == handID) {
                    token.type = TokenType.HAND;
                } else if (word == controlID) {
                    token.type = TokenType.CONTROL;
                } else {
                    token.value = word;
                }
                tok.Add(token);
            }

            return tok;
        }
    }
}
