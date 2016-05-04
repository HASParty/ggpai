using UnityEngine;
using System.Collections;

namespace Boardgame.Agent
{
    [System.Serializable]
    public struct MoodEffect {
        public float Positive;
        public float Negative;
        public float Surprising;
        public float Expected;
    }

    public class Mood : MonoBehaviour
    {

        private Decayable valence;
        private Decayable arousal;

        private float currentValenceMod;
        private float currentArousalMod;

        private MoodEffect mods;

        private PersonalityModule pm;

        //find good defaults
        [SerializeField]
        private float valenceDecay = 0.05f;
        [SerializeField]
        private float arousalDecay = 0.05f;

        void Start() {
            pm = GetComponent<PersonalityModule>();
            float valenceNeutral, arousalNeutral;
            pm.GetMoodConfig(out valenceNeutral, out arousalNeutral, out mods);
            valence = new Decayable(valenceNeutral, 0f, 2f, valenceDecay);
            arousal = new Decayable(arousalNeutral, 0f, 2f, arousalDecay);
        }

        void Update() {
            valence.Update();
            arousal.Update();
            currentValenceMod = (valence.IsPositive() ? mods.Positive : mods.Negative);
            currentArousalMod = (arousal.IsPositive() ? mods.Surprising : mods.Expected);
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
