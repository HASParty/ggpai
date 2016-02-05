namespace Fml
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;

    public class Identikit : MonoBehaviour
    {
        public int id;  // Unique identifier
        public string actorName;
        public Gender gender;

        public Extraversion extraversion;
        public Agreeableness agreeableness;
        public Neuroticism neuroticism;
        public Conscientiousness conscientiousness;
        public Openness openness;
        //public Dominance dominance;

        // Use this for initialization
        void Awake()
        {
        }
    }
}