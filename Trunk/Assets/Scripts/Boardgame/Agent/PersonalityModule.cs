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
        public struct MoodEffect {
            public float Positive;
            public float Negative;
            public float Surprising;
            public float Expected;
        }
        [System.Serializable]
        public struct Modifier {
            public MoodEffect High;
            public MoodEffect Low;
        }
        [System.Serializable]
        public struct Interpolation {
            public Mood High;
            public Mood Low;
        }
        [System.Serializable]
        public struct Trait {
            [HideInInspector]
            public int value;
            /// <summary>
            /// Damp/exaggerate mood depending on trait value and quadrant
            /// </summary>
            public Modifier baseMoodMod;
            /// <summary>
            /// Add/sub mood decay depending on trait value and quadrant
            /// </summary>
            public Modifier moodDecayAddSub;

            public Modifier negativeReactionMod;
            public Modifier positiveReactionMod;
            public Modifier confusedReactionMod;

            /// <summary>
            /// Default mood deviation from neutral
            /// </summary>
            public Interpolation interpolation;
        }

        [SerializeField]
        private Mood mood;
        [SerializeField]
        private float restingArousal;
        [SerializeField]
        private float restingValence;

        [SerializeField]
        private float arousalBaseDecayRate = 0.005f;
        [SerializeField]
        private float valenceBaseDecayRate = 0.005f;

        private MoodEffect decayRate;
        private MoodEffect effect;
        private MoodEffect positive;
        private MoodEffect negative;
        private MoodEffect confused;

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

        [SerializeField]
        private Identikit identikit;

        // Use this for initialization
        private void Start() {
            Low = Config.Low;
            Neutral = Config.Neutral;
            High = Config.High;
            traits[0] = agreeableness;
            traits[1] = conscientiousness;
            traits[2] = extraversion;
            traits[3] = neuroticism;
            traits[4] = openness;
            ReloadPersonality();
            Recalc();
        }

        #region recalculations and initialisations

        public void Recalc() {
            recalcRestingMood();
            resetMood();
            recalcDecayRate();
            recalcWeights();
        }

        private void resetMood() {
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

            positive.Expected = getArousalPositiveFor(Low);
            positive.Surprising = getArousalPositiveFor(High);
            positive.Positive = getValencePositiveFor(High);
            positive.Negative = getValencePositiveFor(Low);

            negative.Expected = getArousalNegativeFor(Low);
            negative.Surprising = getArousalNegativeFor(High);
            negative.Positive = getValenceNegativeFor(High);
            negative.Negative = getValenceNegativeFor(Low);

            confused.Expected = getArousalConfusedFor(Low);
            confused.Surprising = getArousalConfusedFor(High);
            confused.Positive = getValenceConfusedFor(High);
            confused.Negative = getValenceConfusedFor(Low);
        }

        private void recalcDecayRate() {
            decayRate.Surprising = getArousalDecayFor(High);
            decayRate.Expected = getArousalDecayFor(Low);
            decayRate.Negative = getValenceDecayFor(Low);
            decayRate.Positive = getValenceDecayFor(High);
        }

        #endregion

        #region get current values for various things

        public float GetArousalPositiveMod() {
            if (mood.arousal < Neutral) return positive.Expected;
            return positive.Surprising;
        }

        public float GetArousalNegativeMod() {
            if (mood.arousal < Neutral) return negative.Expected;
            return negative.Surprising;
        }

        public float GetArousalConfusedMod() {
            if (mood.arousal < Neutral) return confused.Expected;
            return confused.Surprising;
        }

        public float GetValencePositiveMod() {
            if (mood.valence < Neutral) return positive.Negative;
            return positive.Positive;
        }

        public float GetValenceNegativeMod() {
            if (mood.valence < Neutral) return negative.Negative;
            return negative.Positive;
        }

        public float GetValenceConfusedMod() {
            if (mood.valence < Neutral) return confused.Negative;
            return confused.Positive;
        }

        private float getCurrentArousalEffectWeight() {
            if (mood.valence < Neutral) return effect.Negative;
            return effect.Positive;
        }

        private float getCurrentValenceEffectWeight() {
            if (mood.valence < Neutral) return effect.Negative;
            return effect.Positive;
        }

        public float getCurrentArousalDecay() {
            if (mood.arousal < Neutral) return decayRate.Expected;
            return decayRate.Surprising;
        }

        public float getCurrentValenceDecay() {
            if (mood.valence < Neutral) return decayRate.Negative;
            return decayRate.Positive;
        }

        public float GetArousal() {
            return mood.arousal * getCurrentArousalEffectWeight();
        }

        public float GetValence() {
            return mood.valence * getCurrentValenceEffectWeight();
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

        private float getArousalPositiveFor(float arousal) {
            float total = 1f;
            foreach (Trait t in traits) {
                total *= getLowHighArousal(t.value, arousal, t.positiveReactionMod);
            }
            return total;
        }

        private float getArousalNegativeFor(float arousal) {
            float total = 1f;
            foreach (Trait t in traits) {
                total *= getLowHighArousal(t.value, arousal, t.negativeReactionMod);
            }
            return total;
        }

        private float getArousalConfusedFor(float arousal) {
            float total = 1f;
            foreach (Trait t in traits) {
                total *= getLowHighArousal(t.value, arousal, t.confusedReactionMod);
            }
            return total;
        }

        private float getValencePositiveFor(float valence) {
            float total = 1f;
            foreach (Trait t in traits) {
                total *= getLowHighValence(t.value, valence, t.positiveReactionMod);
            }
            return total;
        }

        private float getValenceNegativeFor(float valence) {
            float total = 1f;
            foreach (Trait t in traits) {
                total *= getLowHighValence(t.value, valence, t.negativeReactionMod);
            }
            return total;
        }

        private float getValenceConfusedFor(float valence) {
            float total = 1f;
            foreach (Trait t in traits) {
                total *= getLowHighValence(t.value, valence, t.confusedReactionMod);
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

        private float getArousalDecayFor(float arousal) {
            float total = 0f;
            foreach (Trait t in traits) {
                total += getLowHighArousal(t.value, arousal, t.moodDecayAddSub, 0);
            }
            return total;
        }

        private float getValenceDecayFor(float valence) {
            float total = 0f;
            foreach (Trait t in traits) {
                total += getLowHighValence(t.value, valence, t.moodDecayAddSub, 0);
            }
            return total;
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


        private void Update() {
            //Mood decay, mood strays back to neutral gradually. 
            float dt = Time.deltaTime;
            int arousalMod = (mood.arousal < restingArousal ? 1 : -1);
            int valenceMod = (mood.valence < restingValence ? 1 : -1);

            mood.arousal += arousalMod * dt * getCurrentArousalDecay();
            mood.valence += valenceMod * dt * getCurrentValenceDecay();

            mood.arousal = Mathf.Clamp(mood.arousal, Low, High);
            mood.valence = Mathf.Clamp(mood.valence, Low, High);

        }

        public void Evaluate(float positivity, float suddenness) {
            mood.valence += positivity / 100;
            mood.arousal += suddenness / 100;
        }
    }
}
