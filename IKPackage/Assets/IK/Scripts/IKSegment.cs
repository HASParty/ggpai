using UnityEngine;
using System.Collections;
namespace IK {
    public class IKSegment : MonoBehaviour {
        public Vector3 RotationConstraintsMax;
        public Vector3 RotationConstraintsMin;
        public float DampDegrees;

        public bool Constrain = true;

        private Quaternion originalRotation;
        private Quaternion lastRotation;
        private Quaternion targetRotation;

        void Awake() {
            originalRotation = transform.rotation;
        }

        public Quaternion GetBaseRotation() {
            return originalRotation;
        }

        public void SetTargetRotation(Quaternion newTarget) {
            targetRotation = newTarget;
            lastRotation = transform.localRotation;
            //Debug.LogFormat("wanna rot {0} from {1}", targetRotation.eulerAngles, lastRotation.eulerAngles);
        }

        public void RotateStep(float t) {
            transform.localRotation = Quaternion.Lerp(lastRotation, targetRotation, t);
        }
    }
}