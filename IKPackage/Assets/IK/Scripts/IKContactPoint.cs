using UnityEngine;

namespace IK {
    [System.Serializable]
    public class IKContactPoint {
        public enum Fingers {
            Thumb,
            Index,
            Middle,
            Ring,
            Pinky
        }
        public Vector3 Location;
        public Vector3 Normal;
        public Fingers Finger;
    }
}
