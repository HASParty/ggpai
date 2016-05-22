using UnityEngine;
using System.Collections;
using Boardgame.Configuration;

namespace Boardgame.Agent {
    public class Mood : MonoBehaviour {

        private Decayable valence;
        private Decayable arousal;

        [SerializeField]
        private float currentValenceMod;
        [SerializeField]
        private float currentArousalMod;

        private MoodEffect mods;

        private PersonalityModule pm;

        //find good defaults
        [SerializeField]
        private float valenceDecay = 0.005f;
        [SerializeField]
        private float arousalDecay = 0.005f;

        void Start() {
            pm = GetComponent<PersonalityModule>();
            float valenceNeutral, arousalNeutral;
            pm.GetMoodConfig(out valenceNeutral, out arousalNeutral, out mods);
            valence = new Decayable(Config.Neutral, valenceNeutral, 0f, 2f, valenceDecay);
            arousal = new Decayable(Config.Neutral, arousalNeutral, 0f, 2f, arousalDecay);
        }

        void Update() {
            valence.Update();
            arousal.Update();
            currentValenceMod = (valence.IsPositive() ? mods.Positive : (valence.IsNegative() ? mods.Negative : 1));
            currentArousalMod = (arousal.IsPositive() ? mods.Surprising : (arousal.IsNegative() ? mods.Expected : 1));
        }

        public void Reset() {
            valence.Reset();
            arousal.Reset();
        }

        public float GetValenceRaw() {
            return valence.Get();
        }

        public float GetArousalRaw() {
            return arousal.Get();
        }

        public float GetValence() {
            return GetValenceRaw() * currentValenceMod;
        }

        public float GetArousal() {
            return GetArousalRaw() * currentArousalMod;
        }

        public void Evaluate(float positivity, float suddenness) {
            valence.Add(positivity / 100);
            arousal.Add(suddenness / 100);
        }
    }
}
