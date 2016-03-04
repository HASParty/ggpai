using Boardgame.GDL;
using System.Collections.Generic;
using UnityEngine;

namespace Boardgame.Networking {
    public class FeedData {
        public int evaluation = 1;

        public FeedData(string parse) {
            var moves = BoardgameManager.Instance.reader.GetConsideredMoves(parse);
            Debug.Log(Tools.Stringify<ConsideredMove>.List(moves, ", "));
        }
    }
}
