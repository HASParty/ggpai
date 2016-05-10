using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boardgame.GDL {
    public struct Message {
        public string action;
        public string legalMoves;
        public string terminal;
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
                m.terminal = "false";
                return m;
            }
            if (split.Length < 3) throw new Exception("GDL Parser: malformed message: "+message);
            int legind = 2, stateind = 3;
            if (split.Length == 3)
            {
                legind--;
                stateind--;
                m.terminal = "false";
            }
            m.action = split[0];
            m.terminal = split[1].Trim();
            m.legalMoves = split[legind].Substring(1, split[legind].Length - 2);
            m.state = split[stateind].Substring(1, split[stateind].Length - 2).Replace(",", "");
            return m;
        }

        public static string CleanMessage(string message) {
            return message.Replace(",", " ").Replace("[", " ").Replace("]", " ")
                .Replace("("," ( ").Replace(")", " ) ").Replace(":",": ");
        }
    }
}
