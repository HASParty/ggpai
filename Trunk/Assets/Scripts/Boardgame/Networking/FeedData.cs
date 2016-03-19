using Boardgame.GDL;
using System;
using System.Linq;

namespace Boardgame.Networking {
    public class FeedData {
        public Move Best;
        public float FirstUCT;
        public float SecondUCT;
        public int MaxSimulation;
        public float SimulationStdDev;

        public FeedData(string parse) {
            var moves = BoardgameManager.Instance.reader.GetConsideredMoves(parse);     
            foreach(var cm in moves) {
                if (cm.Simulations > MaxSimulation) {
                    Best = (cm.First != null ? cm.First : cm.Second);
                    MaxSimulation = cm.Simulations;                    
                    FirstUCT = cm.FirstUCT;
                    SecondUCT = cm.SecondUCT;
                }
            }

            double avgSim = moves.Average(v => v.Simulations);
            double stdDev = Math.Sqrt(moves.Average(v => Math.Pow(v.Simulations - avgSim, 2)));
            SimulationStdDev = (MaxSimulation - (float)avgSim) / (float)stdDev;
        }
    }
}
