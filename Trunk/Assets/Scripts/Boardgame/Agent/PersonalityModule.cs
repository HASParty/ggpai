using UnityEngine;

using System.Collections;
using Fml;

namespace Boardgame.Agent
{
    [SerializePrivateVariables]
    [RequireComponent(typeof(Identikit))]
    public class PersonalityModule : MonoBehaviour
    {
        [System.Serializable]
        public struct Trait
        {
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

        private Identikit identikit;
        private float arousalBaseDecayRate = 0.005f;
        private float valenceBaseDecayRate = 0.005f;
        private float arousalDecayRate;
        private float valenceDecayRate;

        public static readonly int Low = 0;
        public static readonly int Neutral = 1;
        public static readonly int High = 2;

        private Trait agreeableness;
        private Trait conscientiousness;
        private Trait extraversion;
        private Trait neuroticism;
        private Trait openness;

        // Use this for initialization
        void Start()
        {
            RecalcDecayRate();
            RecalcRestingMood();
            ResetMood();
            ReloadIdentikit();       
        }

        public void ResetMood()
        {
            mood.valence = restingValence;
            mood.arousal = restingArousal;
        }

        public void ReloadIdentikit()
        {
            if(identikit == null) identikit = GetComponent<Identikit>();
            agreeableness.value = GetValue(identikit.agreeableness.ToString());
            conscientiousness.value = GetValue(identikit.conscientiousness.ToString());
            extraversion.value = GetValue(identikit.extraversion.ToString());
            neuroticism.value = GetValue(identikit.neuroticism.ToString());
            openness.value = GetValue(identikit.openness.ToString());
        }

        float GetArousalEffectWeight(Trait trait)
        {
            if (trait.value == Low)
            {
                return trait.arousalEffectLow;
            }
            else if (trait.value == High)
            {
                return trait.arousalEffectHigh;
            }

            return 1f;
        }

        float GetValenceEffectWeight(Trait trait)
        {
            if (trait.value == Low)
            {
                return trait.valenceEffectLow;
            }
            else if (trait.value == High)
            {
                return trait.valenceEffectHigh;
            }

            return 1f;
        }

        int GetValue(string val)
        {
            val = val.ToLower();
            if (val == "low") return Low;
            else if (val == "neutral") return Neutral;
            else return High;
        }

        float GetArousalInterpolation(Trait trait)
        {
            if (trait.value == Low)
            {
                return (0.5f - trait.arousalInterpolation) * 0.2f;
            } else if (trait.value == High)
            {
                return (0.5f + trait.arousalInterpolation) * 0.2f;
            }

            return 0.5f * 0.2f;
        }

        float GetValenceInterpolation(Trait trait)
        {
            if (trait.value == Low)
            {
                return (0.5f - trait.valenceInterpolation) * 0.2f;
            }
            else if (trait.value == High)
            {
                return (0.5f + trait.valenceInterpolation) * 0.2f;
            }

            return 0.5f * 0.2f;
        }

        public void RecalcRestingMood()
        {
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

        float GetArousalDecay(Trait trait)
        {
            if (trait.value == Low)
            {
                return -trait.arousalDecay;
            }
            else if (trait.value == High)
            {
                return trait.arousalDecay;
            }

            return 0;
        }

        float GetValenceDecay(Trait trait)
        {
            if (trait.value == Low)
            {
                return -trait.valenceDecay;
            }
            else if (trait.value == High)
            {
                return trait.valenceDecay;
            }

            return 0;
        }

        public void RecalcDecayRate()
        {
            arousalDecayRate =  GetArousalDecay(agreeableness) +
                                GetArousalDecay(conscientiousness) +
                                GetArousalDecay(extraversion) +
                                GetArousalDecay(neuroticism) +
                                GetArousalDecay(openness) +
                                arousalBaseDecayRate;
            valenceDecayRate =  GetValenceDecay(agreeableness) +
                                GetValenceDecay(conscientiousness) +
                                GetValenceDecay(extraversion) +
                                GetValenceDecay(neuroticism) +
                                GetValenceDecay(openness) +
                                valenceBaseDecayRate;
        }

        public float GetArousal()
        {
            return mood.arousal;
        }

        public float GetValence()
        {
            return mood.valence;
        }

        // Update is called once per frame
        void Update()
        {
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

        public EmotionFunction.EmotionalState GetEmotion()
        {
            //MAP EMOTIONS
            return EmotionFunction.EmotionalState.NEUTRAL;
        }

        public void ReceiveEvent(Event e)
        {
            //TODO: define custom event system
            //TODO: receive events and modify mood accordingly
        }


        //will we need these?
        public float GetExtraversion()
        {
            //TODO: determine personality value based on trait degree and current mood
            return 0f;
        }

        public float GetConscientousness()
        {
            //TODO: determine personality value based on trait degree and current mood
            return 0f;
        }

        public float GetAgreeableness()
        {
            //TODO: determine personality value based on trait degree and current mood
            return 0f;
        }

        public float GetNeuroticism()
        {
            //TODO: determine personality value based on trait degree and current mood
            return 0f;
        }

        public float GetOpenness()
        {
            //TODO: determine personality value based on trait degree and current mood
            return 0f;
        }
    }
}
