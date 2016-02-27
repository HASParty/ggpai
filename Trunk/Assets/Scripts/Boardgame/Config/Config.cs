using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Boardgame.Configuration {
    public static class Config {
        public static string MatchID = "1234";
        public static string GameName = "mylla";
        public static int StartTime = 60;
        public static int TurnTime = 10;

        public static int Low = 0;
        public static int Neutral = 1;
        public static int High = 2;

        public static int Extraversion = Neutral;
        public static int Agreeableness = Neutral;
        public static int Neuroticism = Neutral;
        public static int Conscientiousness = Neutral;
        public static int Openness = Neutral;

        public static void SetValue(string which, string value) {
            value = value.Trim();
            switch (which) {
                case "GameName":
                    GameName = value;
                    break;
                case "MatchID":
                    MatchID = value;
                    break;
                case "StartTime":
                case "TurnTime":
                    SetInt(which, value);
                    break;
                default:
                    SetInt(which, value, Low, High);
                    break;
            }
        }

        private static void SetInt(string which, string value, int min = 5, int max = 120) {
            int intValue;
            if(!Int32.TryParse(value, out intValue)) {
                Debug.LogError("Integer expected, did not receive integer.");
            }
            intValue = Mathf.Clamp(intValue, min, max);

            switch (which) {
                case "StartTime":
                    StartTime = intValue;
                    break;
                case "TurnTime":
                    TurnTime = intValue;
                    break;
                case "Extraversion":
                    Extraversion = intValue;
                    break;
                case "Agreeableness":
                    Agreeableness = intValue;
                    break;
                case "Neuroticism":
                    Neuroticism = intValue;
                    break;
                case "Conscientiousness":
                    Conscientiousness = intValue;
                    break;
                case "Openness":
                    Openness = intValue;
                    break;
            }
        }
    }
}
