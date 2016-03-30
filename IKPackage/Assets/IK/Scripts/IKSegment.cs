using UnityEngine;
using System.Collections;
namespace IK {
    public class IKSegment : MonoBehaviour {
        [SerializeField]
        private Vector3 RotationConstraints;

        private Quaternion originalRotation;
        private Quaternion lastRotation;
        private Quaternion targetRotation;

        void Awake() {
            originalRotation = transform.rotation;
        }

        public Quaternion GetConstraints() {
            return Quaternion.Euler(RotationConstraints);
        }

        public void SetTargetRotation(Quaternion newTarget) {
            targetRotation = newTarget;
            lastRotation = transform.rotation;
            Debug.LogFormat("wanna rot {0} from {1}", targetRotation.eulerAngles, lastRotation.eulerAngles);
            transform.rotation = targetRotation;
        }

        public void RotateStep(float t) {
            transform.rotation = Quaternion.Lerp(lastRotation, targetRotation, t);
        }
    }
}