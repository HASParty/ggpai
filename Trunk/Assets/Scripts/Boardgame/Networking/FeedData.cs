using Boardgame.GDL;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Boardgame.Networking {
    public class FeedData {
        public struct FMove {
            public Move Move;
            public int Simulations;
            public Player Who;
            public float FirstUCT;
            public float SecondUCT;
            public override string ToString() {
                return string.Format("{0} s: {1} p: {2} fUCT: {3} sUCT: {4}",
                    Move, Simulations, Who, FirstUCT, SecondUCT);
            }
        }
        public Move Best;
        public Dictionary<Move, FMove> Moves = new Dictionary<Move, FMove>();
        public float SimulationStdDev;
        public double AverageSimulations;
        public int TotalSimulations;
        public float FirstWeightedUCT;
        public float SecondWeightedUCT;

        public FeedData(string parse) {
            int maxsim = 0;
            var moves = BoardgameManager.Instance.reader.GetConsideredMoves(parse);
            TotalSimulations = moves.Sum(v => v.Simulations);
            FirstWeightedUCT = 0;
            SecondWeightedUCT = 0;
            foreach(var cm in moves) {
                if (cm.Simulations > maxsim) {
                    Best = (cm.First != null ? cm.First : cm.Second);
                    maxsim = cm.Simulations;                    
                }
                FMove m;
                m.Move = (cm.First != null ? cm.First : cm.Second);
                m.FirstUCT = cm.FirstUCT;
                m.SecondUCT = cm.SecondUCT;
                m.Who = (cm.First != null ? Player.First : Player.Second);
                m.Simulations = cm.Simulations;
                if (TotalSimulations > 0) {
                    float weight = (float)m.Simulations / (float)TotalSimulations;
                    FirstWeightedUCT += m.FirstUCT * weight;
                    SecondWeightedUCT += m.SecondUCT * weight;
                }
                Moves.Add(m.Move, m);
            }

            try {
                AverageSimulations = moves.Average(v => v.Simulations);                
                double stdDev = Math.Sqrt(moves.Average(v => Math.Pow(v.Simulations - AverageSimulations, 2)));
                SimulationStdDev = (maxsim - (float)AverageSimulations) / (float)stdDev;
            } catch (Exception e) {
                Debug.LogWarning(e);
            }
        }
    }
}
