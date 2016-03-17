using Boardgame.GDL;
using System.Collections.Generic;
using UnityEngine;

namespace Boardgame.Networking {
    public class FeedData {
        public Move Best;
        public float FirstUCT;
        public float SecondUCT;
        public int MaxSimulation;

        public FeedData(string parse) {
            var moves = BoardgameManager.Instance.reader.GetConsideredMoves(parse);
            MaxSimulation = -1;
            foreach(var cm in moves) {
                if(cm.Simulations > MaxSimulation) {
                    Best = (cm.First != null ? cm.First : cm.Second);
                    MaxSimulation = cm.Simulations;
                    FirstUCT = cm.FirstUCT;
                    SecondUCT = cm.SecondUCT;
                }
            }
        }
    }
}
