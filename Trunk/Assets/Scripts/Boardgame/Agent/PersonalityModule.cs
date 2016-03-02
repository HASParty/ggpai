using UnityEngine;

using System.Collections;
using FML;
using Boardgame.Configuration;

namespace Boardgame.Agent {
    [SerializePrivateVariables]
    public class PersonalityModule : MonoBehaviour {
        [System.Serializable]
        public struct Trait {
            public int value;
            //how much the mood swings as a result
            //of events when trait high
            public float valenceEffectHigh;
            public float arousalEffectHigh;
            //when trait low
            public float valenceEffectLow;
            public float arousalEffectLow;
            //amount of default deviation from neutral
            public float valenceInterpolation;
            public float arousalInterpolation;
            //add/subtract to decay if high/low
            public float valenceDecay;
            public float arousalDecay;
        }

        private Mood mood;
        private float restingArousal;
        private float restingValence;

        private float arousalBaseDecayRate = 0.005f;
        private float valenceBaseDecayRate = 0.005f;
        private float arousalDecayRate;
        private float valenceDecayRate;

        private int Low;
        private int Neutral;
        private int High;

        private Trait agreeableness;
        private Trait conscientiousness;
        private Trait extraversion;
        private Trait neuroticism;
        private Trait openness;

        private Identikit identikit;

        // Use this for initialization
        void Start() {
            Low = Config.Low;
            Neutral = Config.Neutral;
            High = Config.High;
            RecalcDecayRate();
            RecalcRestingMood();
            ResetMood();
            ReloadPersonality();
        }

        public void ResetMood() {
            mood.valence = restingValence;
            mood.arousal = restingArousal;
        }

        public void ReloadPersonality() {
            agreeableness.value = Config.Agreeableness;
            conscientiousness.value = Config.Conscientiousness;
            extraversion.value = Config.Extraversion;
            neuroticism.value = Config.Neuroticism;
            openness.value = Config.Openness;

            if (identikit == null) {
                identikit = gameObject.AddComponent<Identikit>();
            }

            identikit.SetValues(extraversion.value, agreeableness.value, neuroticism.value, conscientiousness.value, openness.value);


        }

        float GetArousalEffectWeight(Trait trait) {
            if (trait.value == Low) {
                return trait.arousalEffectLow;
            } else if (trait.value == High) {
                return trait.arousalEffectHigh;
            }

            return 1f;
        }

        float GetValenceEffectWeight(Trait trait) {
            if (trait.value == Low) {
                return trait.valenceEffectLow;
            } else if (trait.value == High) {
                return trait.valenceEffectHigh;
            }

            return 1f;
        }

        float GetArousalInterpolation(Trait trait) {
            if (trait.value == Low) {
                return (0.5f - trait.arousalInterpolation) * 0.2f;
            } else if (trait.value == High) {
                return (0.5f + trait.arousalInterpolation) * 0.2f;
            }

            return 0.5f * 0.2f;
        }

        float GetValenceInterpolation(Trait trait) {
            if (trait.value == Low) {
                return (0.5f - trait.valenceInterpolation) * 0.2f;
            } else if (trait.value == High) {
                return (0.5f + trait.valenceInterpolation) * 0.2f;
            }

            return 0.5f * 0.2f;
        }

        public void RecalcRestingMood() {
            float arousalInterpolate = GetArousalInterpolation(agreeableness) +
                                       GetArousalInterpolation(conscientiousness) +
                                       GetArousalInterpolation(extraversion) +
                                       GetArousalInterpolation(neuroticism) +
                                       GetArousalInterpolation(openness);
            float valenceInterpolate = GetValenceInterpolation(agreeableness) +
                                       GetValenceInterpolation(conscientiousness) +
                                       GetValenceInterpolation(extraversion) +
                                       GetValenceInterpolation(neuroticism) +
                                       GetValenceInterpolation(openness);
            restingArousal = Mathf.Lerp(Low, High, Mathf.Clamp01(arousalInterpolate));
            restingValence = Mathf.Lerp(Low, High, Mathf.Clamp01(valenceInterpolate));
        }

        float GetArousalDecay(Trait trait) {
            if (trait.value == Low) {
                return -trait.arousalDecay;
            } else if (trait.value == High) {
                return trait.arousalDecay;
            }

            return 0;
        }

        float GetValenceDecay(Trait trait) {
            if (trait.value == Low) {
                return -trait.valenceDecay;
            } else if (trait.value == High) {
                return trait.valenceDecay;
            }

            return 0;
        }

        public void RecalcDecayRate() {
            arousalDecayRate = GetArousalDecay(agreeableness) +
                                GetArousalDecay(conscientiousness) +
                                GetArousalDecay(extraversion) +
                                GetArousalDecay(neuroticism) +
                                GetArousalDecay(openness) +
                                arousalBaseDecayRate;
            valenceDecayRate = GetValenceDecay(agreeableness) +
                                GetValenceDecay(conscientiousness) +
                                GetValenceDecay(extraversion) +
                                GetValenceDecay(neuroticism) +
                                GetValenceDecay(openness) +
                                valenceBaseDecayRate;
        }

        public float GetArousal() {
            return mood.arousal;
        }

        public float GetValence() {
            return mood.valence;
        }

        // Update is called once per frame
        void Update() {
            //Mood decay, mood strays back to neutral gradually. 
            //Values need to be tested to determine best rate.
            float dt = Time.deltaTime;
            int arousalMod = (mood.arousal < restingArousal ? 1 : -1);
            int valenceMod = (mood.valence < restingValence ? 1 : -1);

            mood.arousal += arousalMod * dt * arousalDecayRate;
            mood.valence += valenceMod * dt * valenceDecayRate;

            Mathf.Clamp(mood.arousal, Low, High);
            Mathf.Clamp(mood.valence, Low, High);
        }

        public void ReceiveEvent(Event e) {
            //TODO: define custom event system
            //TODO: receive events and modify mood accordingly
        }


        //will we need these?
        public float GetExtraversion() {
            //TODO: determine personality value based on trait degree and current mood
            return 0f;
        }

        public float GetConscientousness() {
            //TODO: determine personality value based on trait degree and current mood
            return 0f;
        }

        public float GetAgreeableness() {
            //TODO: determine personality value based on trait degree and current mood
            return 0f;
        }

        public float GetNeuroticism() {
            //TODO: determine personality value based on trait degree and current mood
            return 0f;
        }

        public float GetOpenness() {
            //TODO: determine personality value based on trait degree and current mood
            return 0f;
        }
    }
}
