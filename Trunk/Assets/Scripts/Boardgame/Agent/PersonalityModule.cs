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
            public float valenceInterpolation;
            public float arousalInterpolation;
            public float valenceInterpolationWeight;
            public float arousalInterpolationWeight;
            public float valenceDecayAdd;
            public float valenceDecayMod;
            public float arousalDecayAdd;
            public float arousalDecayMod;
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

        int GetValue(string val)
        {
            val = val.ToLower();
            if (val == "low") return Low;
            else if (val == "neutral") return Neutral;
            else return High;
        }

        public void RecalcRestingMood()
        {
            float arousalInterpolate = agreeableness.arousalInterpolation * agreeableness.arousalInterpolationWeight +
                                       conscientiousness.arousalInterpolation * conscientiousness.arousalInterpolationWeight +
                                       extraversion.arousalInterpolation * extraversion.arousalInterpolationWeight +
                                       neuroticism.arousalInterpolation * neuroticism.arousalInterpolationWeight +
                                       openness.arousalInterpolation * openness.arousalInterpolationWeight;
            float valenceInterpolate = agreeableness.valenceInterpolation * agreeableness.valenceInterpolationWeight +
                                       conscientiousness.valenceInterpolation * conscientiousness.valenceInterpolationWeight +
                                       extraversion.valenceInterpolation * extraversion.valenceInterpolationWeight +
                                       neuroticism.valenceInterpolation * neuroticism.valenceInterpolationWeight +
                                       openness.valenceInterpolation * openness.valenceInterpolationWeight;
            restingArousal = Mathf.Lerp(Low, High, Mathf.Clamp01(arousalInterpolate));
            restingValence = Mathf.Lerp(Low, High, Mathf.Clamp01(valenceInterpolate));
        }

        public void RecalcDecayRate()
        {
            arousalDecayRate = agreeableness.arousalDecayAdd + conscientiousness.arousalDecayAdd +
                               extraversion.arousalDecayAdd + neuroticism.arousalDecayAdd + openness.arousalDecayAdd +
                               arousalBaseDecayRate * agreeableness.arousalDecayMod * conscientiousness.arousalDecayMod *
                               extraversion.arousalDecayMod * neuroticism.arousalDecayMod * openness.arousalDecayMod;
            valenceDecayRate = agreeableness.valenceDecayAdd + conscientiousness.valenceDecayAdd +
                               extraversion.valenceDecayAdd + neuroticism.valenceDecayAdd + openness.valenceDecayAdd +
                               valenceBaseDecayRate * agreeableness.valenceDecayMod * conscientiousness.valenceDecayMod *
                               extraversion.valenceDecayMod * neuroticism.valenceDecayMod * openness.valenceDecayMod;
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
            float angle = Vector2.Angle(new Vector2(Neutral, High), new Vector2(mood.arousal, mood.valence));
            Debug.Log(angle);
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
