using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IK {
    [System.Serializable]
    public class IKTiming {
        public IKTarget Target;
        [Range(0f, 1f)]
        public float Timing;
        public bool GrabTarget;

        public IKTiming(IKTarget target, float timing, bool grab = false) {
            Timing = Mathf.Clamp01(timing);
            Target = target;
            GrabTarget = grab;
        }
    }
}
