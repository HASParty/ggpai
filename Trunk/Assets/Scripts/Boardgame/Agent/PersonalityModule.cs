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

        private float arousalSurpriseDecayRate;
        private float arousalCalmDecayRate;
        private float valenceNegativeDecayRate;
        private float valencePositiveDecayRate;

        private float arousalSurpriseEffect;
        private float arousalCalmEffect;
        private float valenceNegativeEffect;
        private float valencePositiveEffect;

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

        public void Recalc() {
            recalcRestingMood();
            resetMood();
            recalcDecayRate();
            recalcEffectWeights();
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

        private void recalcEffectWeights() {
            arousalCalmEffect = getArousalEffectWeightFor(0f);
            arousalSurpriseEffect = getArousalEffectWeightFor(2f);
            valencePositiveEffect = getValenceEffectWeightFor(2f);
            valenceNegativeEffect = getValenceEffectWeightFor(0f);
        }

        private float getCurrentArousalEffectWeight() {
            if (mood.arousal < Neutral) return arousalCalmEffect;
            return arousalSurpriseEffect;
        }

        private float getCurrentValenceEffectWeight() {
            if (mood.valence < Neutral) return valenceNegativeEffect;
            return valencePositiveEffect;
        }

        private float getValenceEffectWeightFor(float valence) {
            return GetValenceEffectWeight(agreeableness, valence) *
               GetValenceEffectWeight(conscientiousness, valence) *
               GetValenceEffectWeight(extraversion, valence) *
               GetValenceEffectWeight(neuroticism, valence) *
               GetValenceEffectWeight(openness, valence);
        }

        private float getArousalEffectWeightFor(float arousal) {
            return GetArousalEffectWeight(agreeableness, arousal) *
               GetArousalEffectWeight(conscientiousness, arousal) *
               GetArousalEffectWeight(extraversion, arousal) *
               GetArousalEffectWeight(neuroticism, arousal) *
               GetArousalEffectWeight(openness, arousal);
        }

        /*private float getByArousal(float value, Modifier mod, float arousal, float retdefault = 1f) {
            if(value == Low) {
                return (arousal < Neutral ? mod.Low.Negative : baseMoodMod.Low.Positive;
            } else if (value == High) {

            }

            return retdefault;
        }*/

        private float GetArousalEffectWeight(Trait trait, float arousal) {
            if (trait.value == Low) {
                return (arousal < Neutral ? trait.baseMoodMod.Low.Expected : trait.baseMoodMod.Low.Surprising);
            } else if (trait.value == High) {
                return (arousal < Neutral ? trait.baseMoodMod.High.Expected : trait.baseMoodMod.High.Surprising);
            }

            return 1f;
        }

        private float GetValenceEffectWeight(Trait trait, float valence) {
            if (trait.value == Low) {
                return (valence < Neutral ? trait.baseMoodMod.Low.Negative : trait.baseMoodMod.Low.Positive);
            } else if (trait.value == High) {
                return (valence < Neutral ? trait.baseMoodMod.High.Negative : trait.baseMoodMod.High.Positive);
            }

            return 1f;
        }

        private float GetArousalInterpolation(Trait trait) {
            if (trait.value == Low) {
                return (0.5f + trait.interpolation.Low.arousal) * 0.2f;
            } else if (trait.value == High) {
                return (0.5f + trait.interpolation.High.arousal) * 0.2f;
            }

            return 0.5f * 0.2f;
        }

        private float GetValenceInterpolation(Trait trait) {
            if (trait.value == Low) {
                return (0.5f + trait.interpolation.Low.valence) * 0.2f;
            } else if (trait.value == High) {
                return (0.5f + trait.interpolation.High.valence) * 0.2f;
            }

            return 0.5f * 0.2f;
        }

        private void recalcRestingMood() {
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

        private float getArousalDecay(Trait trait, float arousal) {
            if (trait.value == Low) {
               return (arousal < Neutral ? trait.moodDecayAddSub.Low.Expected : trait.moodDecayAddSub.Low.Surprising);
            } else if (trait.value == High) {
                return (arousal < Neutral ? trait.moodDecayAddSub.High.Expected : trait.moodDecayAddSub.High.Surprising);
            }

            return 0f;
        }

        private float getValenceDecay(Trait trait, float valence) {
            if(trait.value == Low) {
                return (valence < Neutral ? trait.moodDecayAddSub.Low.Negative : trait.moodDecayAddSub.Low.Positive);
            } else if (trait.value == High) {
                return (mood.valence < Neutral ? trait.moodDecayAddSub.High.Negative : trait.moodDecayAddSub.High.Positive);
             }

            return 0f;
        }

        public float getCurrentArousalDecay() {
            if (mood.arousal < Neutral) return arousalCalmDecayRate;
            return arousalSurpriseDecayRate;
        }

        public float getCurrentValenceDecay() {
            if (mood.valence < Neutral) return valenceNegativeDecayRate;
            return valencePositiveDecayRate;
        }

        private float getArousalDecayFor(float arousal) {
            return getArousalDecay(agreeableness, arousal) +
                getArousalDecay(conscientiousness, arousal) +
                getArousalDecay(extraversion, arousal) +
                getArousalDecay(neuroticism, arousal) +
                getArousalDecay(openness, arousal) +
                arousalBaseDecayRate;
        }

        private float getValenceDecayFor(float valence) {
            return getValenceDecay(agreeableness, valence) +
                getValenceDecay(conscientiousness, valence) +
                getValenceDecay(extraversion, valence) +
                getValenceDecay(neuroticism, valence) +
                getValenceDecay(openness, valence) +
                valenceBaseDecayRate;
        }


        private void recalcDecayRate() {
            arousalSurpriseDecayRate = getArousalDecayFor(2f);
            arousalCalmDecayRate = getArousalDecayFor(0f);
            valenceNegativeDecayRate = getValenceDecayFor(0f);
            valencePositiveDecayRate = getValenceDecayFor(2f);
        }

        public float GetArousal() {
            return mood.arousal * getCurrentArousalEffectWeight();
        }

        public float GetValence() {
            return mood.valence * getCurrentValenceEffectWeight();
        }

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
