using UnityEngine;

namespace Boardgame.Agent {
    public class Param {
        public Param(float min, float max, float valenceWeight = 0.5f, float arousalWeight = 0.5f) {
            this.min = min;
            this.max = max;
            float totalWeight = valenceWeight + arousalWeight;
            if (totalWeight > 0f) {
                this.valenceWeight = Mathf.Clamp01(valenceWeight) / totalWeight;
                this.arousalWeight = Mathf.Clamp01(arousalWeight) / totalWeight;
            } else {
                this.valenceWeight = 0;
                this.arousalWeight = 0;
            }
        }

        public float Interpolate(float valence, float arousal) {
            float vint = (valence / 2) * valenceWeight;
            float varo = (arousal / 2) * arousalWeight;

            return Mathf.Lerp(min, max, vint + varo);
        }

        public void Add(float min = 0, float max = 0) {
            this.min += min;
            this.max += max;
        }

        float min;
        float max;
        float valenceWeight;
        float arousalWeight;
    }
}
