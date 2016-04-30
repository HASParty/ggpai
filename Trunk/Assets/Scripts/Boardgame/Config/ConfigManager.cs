using System;
using UnityEngine;

namespace Boardgame.Configuration {
    public class ConfigManager : Singleton<ConfigManager> {
        [SerializeField]
        [Tooltip("Override the values set in inspector with values configured in-game")]
        private bool Override;

        public string MatchID;
        public string GameName;
        public int StartTime;
        public int TurnTime;
        public int Turns;

        public int Low;
        public int Neutral;
        public int High;

        public int SimulationCutoff;

        [HideInInspector]
        public int Extraversion;
        [HideInInspector]
        public int Agreeableness;
        [HideInInspector]
        public int Neuroticism;
        [HideInInspector]
        public int Conscientiousness;
        [HideInInspector]
        public int Openness;


        public void SetConfig() {
            if (Override) {
                Config.MatchID = MatchID;
                Config.GameName = GameName;
                Config.StartTime = StartTime;
                Config.TurnTime = TurnTime;
                Config.Low = Low;
                Config.Neutral = Neutral;
                Config.High = High;
                Config.Turns = Turns;
                Config.Extraversion = Extraversion;
                Config.Agreeableness = Agreeableness;
                Config.Neuroticism = Neuroticism;
                Config.Conscientiousness = Conscientiousness;
                Config.Openness = Openness;
                Config.SimulationCutoff = SimulationCutoff;
            } else {
                MatchID = Config.MatchID;
                GameName = Config.GameName;
                StartTime = Config.StartTime;
                TurnTime = Config.TurnTime;
                Low = Config.Low;
                Neutral = Config.Neutral;
                High = Config.High;
                Turns = Config.Turns;
                Extraversion = Config.Extraversion;
                Agreeableness = Config.Agreeableness;
                Neuroticism = Config.Neuroticism;
                Conscientiousness = Config.Conscientiousness;
                Openness = Config.Openness;
                SimulationCutoff = Config.SimulationCutoff;
            }
        }

        void Awake() {
            SetConfig();
        }
    }
}
