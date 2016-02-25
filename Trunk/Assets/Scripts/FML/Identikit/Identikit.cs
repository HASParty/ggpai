namespace Fml
{
    using Boardgame.Configuration;
    using System;
    using UnityEngine;

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

        public void SetValues(int extrav, int agreeab, int neurot, int conscient, int open) {
        //TODO: set this for fml stuff
        //not really imperative yet   
        }
    }
}