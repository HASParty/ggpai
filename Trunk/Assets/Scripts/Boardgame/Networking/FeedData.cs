using Boardgame.GDL;
using System;
using System.Linq;
using UnityEngine;

namespace Boardgame.Networking {
    public class FeedData {
        public Move Best;
        public Move Worst;
        public float FirstUCT;
        public float SecondUCT;
        public int MaxSimulation;
        public int MinSimulation = int.MaxValue;
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
                if(cm.Simulations < MinSimulation) {
                    MinSimulation = cm.Simulations;
                    Worst = (cm.First != null ? cm.First : cm.Second);
                }
            }

            try {
                double avgSim = moves.Average(v => v.Simulations);
                double stdDev = Math.Sqrt(moves.Average(v => Math.Pow(v.Simulations - avgSim, 2)));
                SimulationStdDev = (MaxSimulation - (float)avgSim) / (float)stdDev;
            } catch (Exception e) {
                Debug.LogWarning(e);
            }
        }
    }
}
