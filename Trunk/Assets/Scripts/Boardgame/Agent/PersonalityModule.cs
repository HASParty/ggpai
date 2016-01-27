using UnityEngine;

using System.Collections;

namespace Boardgame.Agent
{
    public enum PersonalityDegree {
        Low,
        Neutral,
        High
    };

    public class PersonalityModule : MonoBehaviour
    {
        [SerializeField]
        private Mood mood;
        [Header("Personality values")]
        [SerializeField]
        private PersonalityDegree extraversion;
        [SerializeField]
        private PersonalityDegree agreeableness;
        [SerializeField]
        private PersonalityDegree conscientousness;
        [SerializeField]
        private PersonalityDegree neuroticism;
        [SerializeField]
        private PersonalityDegree openness;
        [Header("Settings")]
        [Tooltip("The amount the arousal changes in a second"), SerializeField]
        private float arousalDecayRate = 0.005f;
        [Tooltip("The amount the valence changes in a second"), SerializeField]
        private float valenceDecayRate = 0.005f;
        [SerializeField]
        private float lowVal = 0f;
        [SerializeField]
        private float neutralVal = 1f;
        [SerializeField]
        private float highVal = 2f;

        // Use this for initialization
        void Start()
        {
        }

        public void InitialisePersonality(PersonalityDegree extraversion, 
            PersonalityDegree agreeableness, PersonalityDegree conscientousness,
            PersonalityDegree neuroticism, PersonalityDegree openness)
        {
            this.extraversion = extraversion;
            this.conscientousness = conscientousness;
            this.agreeableness = agreeableness;
            this.neuroticism = neuroticism;
            this.openness = openness;
            mood.arousal = neutralVal;
            mood.valence = neutralVal;
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
