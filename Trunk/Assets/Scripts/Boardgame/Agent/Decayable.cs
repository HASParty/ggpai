using UnityEngine;

namespace Boardgame.Agent {
    public class Decayable {
        private float val;
        private float neutral;
        private float trueNeutral;
        private float min;
        private float max;
        private float decay;

        public Decayable(float trueNeutral, float neutral, float min, float max, float decay) {
            this.neutral = neutral;
            this.trueNeutral = trueNeutral;
            this.min = min;
            this.max = max;
            this.decay = decay;
            Reset();
        }

        public bool IsPositive() {
            return val > trueNeutral + 0.05f;
        }

        public bool IsNegative()
        {
            return val < trueNeutral - 0.05f;
        }

        public void Reset() {
            val = neutral;
        }

        public void Add(float amount) {
            val += amount;
        }

        public float Get() {
            return val;
        }

        public void Update() {            
            int mod = (val < neutral ? 1 : -1);
            val += mod * Time.deltaTime * decay;
            val = Mathf.Clamp(val, min, max);
        }
    }
}
