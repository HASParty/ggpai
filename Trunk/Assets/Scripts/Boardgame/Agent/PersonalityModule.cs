using UnityEngine;

using System.Collections;
using FML;
using Boardgame.Configuration;

namespace Boardgame.Agent {
    /// <summary>
    /// The personality of the agent.
    /// </summary>
    public class PersonalityModule : MonoBehaviour {
        [System.Serializable]
        private struct Modifier {
            public MoodEffect High;
            public MoodEffect Low;
        }
        [System.Serializable]
        private struct MoodVals {
            public float arousal;
            public float valence;
        }
        [System.Serializable]
        private struct Interpolation {
            public MoodVals High;
            public MoodVals Low;
        }
        [System.Serializable]
        private struct Trait {
            [HideInInspector]
            public int value;
            /// <summary>
            /// Damp/exaggerate mood depending on trait value and quadrant
            /// </summary>
            public Modifier baseMoodMod;
            /// <summary>
            /// Default mood deviation from neutral
            /// </summary>
            public Interpolation interpolation;
        }

        private float restingArousal;
        private float restingValence;
        private MoodEffect effect;

        [SerializeField]
        private int Low;
        [SerializeField]
        private int Neutral;
        [SerializeField]
        private int High;

        [SerializeField]
        private Trait agreeableness;
        [SerializeField]
        private Trait conscientiousness;
        [SerializeField]
        private Trait extraversion;
        [SerializeField]
        private Trait neuroticism;
        [SerializeField]
        private Trait openness;

        private Trait[] traits = new Trait[5];

        // Use this for initialization
        private void Awake() {
            Low = Config.Low;
            Neutral = Config.Neutral;
            High = Config.High;
            ReloadPersonality();
            traits[0] = agreeableness;
            traits[1] = conscientiousness;
            traits[2] = extraversion;
            traits[3] = neuroticism;
            traits[4] = openness;
            Recalc();
        }

        #region recalculations and initialisations

        public void Recalc() {
            recalcRestingMood();
            recalcWeights();
        }

        public void GetMoodConfig(out float restingValence, out float restingArousal, out MoodEffect mods) {
            Recalc();
            restingValence = this.restingValence;
            restingArousal = this.restingArousal;
            mods = this.effect;
        }

        public void ReloadPersonality() {
            agreeableness.value = Config.Agreeableness;
            conscientiousness.value = Config.Conscientiousness;
            extraversion.value = Config.Extraversion;
            neuroticism.value = Config.Neuroticism;
            openness.value = Config.Openness;
        }

        private void recalcRestingMood() {
            float arousalInterpolate = getArousalInterpolation(agreeableness) +
                                       getArousalInterpolation(conscientiousness) +
                                       getArousalInterpolation(extraversion) +
                                       getArousalInterpolation(neuroticism) +
                                       getArousalInterpolation(openness);
            float valenceInterpolate = getValenceInterpolation(agreeableness) +
                                       getValenceInterpolation(conscientiousness) +
                                       getValenceInterpolation(extraversion) +
                                       getValenceInterpolation(neuroticism) +
                                       getValenceInterpolation(openness);
            restingArousal = Mathf.Lerp(Low, High, Mathf.Clamp01(arousalInterpolate));
            restingValence = Mathf.Lerp(Low, High, Mathf.Clamp01(valenceInterpolate));
        }


        private void recalcWeights() {
            effect.Expected = getArousalEffectWeightFor(Low);
            effect.Surprising = getArousalEffectWeightFor(High);
            effect.Positive = getValenceEffectWeightFor(High);
            effect.Negative = getValenceEffectWeightFor(Low);
        }

        #endregion

        #region redundant helpers that could be made less redundant by sacrificing legibility
        private float getValenceEffectWeightFor(float valence) {
            float total = 1f;
            foreach (Trait t in traits) {
                total *= getLowHighValence(t.value, valence, t.baseMoodMod);
            }
            return total;
        }

        private float getArousalEffectWeightFor(float arousal) {
            float total = 1f;
            foreach (Trait t in traits) {
                total *= getLowHighArousal(t.value, arousal, t.baseMoodMod);
            }
            return total;
        }

        private float getLowHighArousal(float value, float arousal, Modifier mod, float retDefault = 1f) {
            if (value == Low) {
                return (arousal < Neutral ? mod.Low.Negative : mod.Low.Positive);
            } else if (value == High) {
                return (arousal < Neutral ? mod.High.Negative : mod.High.Positive);
            }

            return retDefault;
        }

        private float getLowHighValence(float value, float valence, Modifier mod, float retDefault = 1f) {
            if (value == Low) {
                return (valence < Neutral ? mod.Low.Expected : mod.Low.Surprising);
            } else if (value == High) {
                return (valence < Neutral ? mod.High.Expected : mod.High.Surprising);
            }

            return retDefault;
        }

        private float getArousalInterpolation(Trait trait) {
            if (trait.value == Low) {
                return (0.5f + trait.interpolation.Low.arousal) * 0.2f;
            } else if (trait.value == High) {
                return (0.5f + trait.interpolation.High.arousal) * 0.2f;
            }

            return 0.5f * 0.2f;
        }

        private float getValenceInterpolation(Trait trait) {
            if (trait.value == Low) {
                return (0.5f + trait.interpolation.Low.valence) * 0.2f;
            } else if (trait.value == High) {
                return (0.5f + trait.interpolation.High.valence) * 0.2f;
            }

            return 0.5f * 0.2f;
        }

        #endregion

        #region traits
        public enum PersonalityValue {
            low,
            neutral,
            high
        }
        private PersonalityValue getVal(Trait t) {
            if (t.value == Low) return PersonalityValue.low;
            else if (t.value == High) return PersonalityValue.high;
            return PersonalityValue.neutral;
        }

        public PersonalityValue GetExtraversion() {
            return getVal(extraversion);
        }
        public PersonalityValue GetOpenness() {
            return getVal(openness);
        }
        public PersonalityValue GetNeuroticism() {
            return getVal(neuroticism);
        }
        public PersonalityValue GetAgreeableness() {
            return getVal(agreeableness);
        }
        public PersonalityValue GetConscientiousness() {
            return getVal(conscientiousness);
        }
        #endregion

    }
}
