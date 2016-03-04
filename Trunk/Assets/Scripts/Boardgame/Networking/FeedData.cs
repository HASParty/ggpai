using Boardgame.GDL;
using System.Collections.Generic;
using UnityEngine;

namespace Boardgame.Networking {
    public class FeedData {
        public Move Best;

        public FeedData(string parse) {
            var moves = BoardgameManager.Instance.reader.GetConsideredMoves(parse);
            int max = -1;
            foreach(var cm in moves) {
                if(cm.Simulations > max) {
                    Best = (cm.First != null ? cm.First : cm.Second);
                    max = cm.Simulations;
                }
            }
        }
    }
}
