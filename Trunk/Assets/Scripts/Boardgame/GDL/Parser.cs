using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boardgame.GDL {
    public struct Message {
        public string action;
        public string legalMoves;
        public string state;
    }
    public class Parser {
        public static Message BreakMessage(string message) {
            var split = message.Split(new char[] { ':' }, 4, StringSplitOptions.RemoveEmptyEntries);
            Message m;
            if (split.Length == 1)
            {
                m.action = "done";
                m.legalMoves = null;
                m.state = null;
                return m;
            }
            if (split.Length < 3) throw new Exception("GDL Parser: malformed message: "+message);
            m.action = split[0];
            m.state = split[1].Trim();
            m.legalMoves = split[2].Substring(1, split[1].Length - 2);
            m.state = split[3].Substring(1, split[2].Length - 2).Replace(",", "");
            return m;
        }

        public static string CleanMessage(string message) {
            return message.Replace(",", " ").Replace("[", " ").Replace("]", " ")
                .Replace("("," ( ").Replace(")", " ) ").Replace(":",": ");
        }
    }
}
