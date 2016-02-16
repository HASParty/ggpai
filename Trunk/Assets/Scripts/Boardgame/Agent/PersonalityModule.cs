using UnityEngine;

using System.Collections;
using Fml;

namespace Boardgame.Agent
{
    [SerializePrivateVariables]
    [RequireComponent(typeof(Identikit))]
    public class PersonalityModule : MonoBehaviour
    {
        private Mood mood;
        private EmotionFunction.EmotionalState approximateEmotion;
        private float emotionIntensity;
        private Identikit identikit;
        private float arousalDecayRate = 0.005f;
        private float valenceDecayRate = 0.005f;
        private float lowVal = 0f;
        private float neutralVal = 1f;
        private float highVal = 2f;

        private float agreeableness,    agreeablenessModifier;
        private float conscientiousness, conscientousnessModifier;
        private float extraversion,     extraversionModifier;
        private float neuroticism,      neuroticismModifier;
        private float openness,         opennessModifier;

        // Use this for initialization
        void Start()
        {
            mood.valence = neutralVal;
            mood.arousal = neutralVal;
            ReloadIdentikit();
        }

        public void ReloadIdentikit()
        {
            if(identikit == null) identikit = GetComponent<Identikit>();
            agreeableness = GetValue(identikit.agreeableness.ToString());
            conscientiousness = GetValue(identikit.conscientiousness.ToString());
            extraversion = GetValue(identikit.extraversion.ToString());
            neuroticism = GetValue(identikit.neuroticism.ToString());
            openness = GetValue(identikit.openness.ToString());
        }

        float GetValue(string val)
        {
            val = val.ToLower();
            if (val == "low") return lowVal;
            else if (val == "neutral") return neutralVal;
            else return highVal;
        }

        // Update is called once per frame
        void Update()
        {
            //Mood decay, mood strays back to neutral gradually. 
            //Values need to be tested to determine best rate.
            float dt = Time.deltaTime;
            int arousalMod = (mood.arousal < neutralVal ? 1 : -1);
            int valenceMod = (mood.valence < neutralVal ? 1 : -1);

            mood.arousal += arousalMod * dt * arousalDecayRate;
            mood.valence += valenceMod * dt * valenceDecayRate;

            Mathf.Clamp(mood.arousal, lowVal, highVal);
            Mathf.Clamp(mood.valence, lowVal, highVal);

#if UNITY_EDITOR
            emotionIntensity = GetIntensity();
            approximateEmotion = GetEmotion();
#endif
        }

        public float GetIntensity()
        {

            //TODO: have 
            return Mathf.Sqrt(Mathf.Pow(mood.arousal-1, 2) + Mathf.Pow(mood.valence-1, 2));
        }

        public EmotionFunction.EmotionalState GetEmotion()
        {
            return EmotionFunction.EmotionalState.NEUTRAL;
        }

        public void ReceiveEvent(Event e)
        {
            //TODO: define custom event system
            //TODO: receive events and modify mood accordingly
        }

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
