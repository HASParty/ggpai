using UnityEngine;
using System.Collections;
namespace IK {
    public class IKSegment : MonoBehaviour {
        public string ID;
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
        }

        public void RotateStep(float t) {
            transform.rotation = Quaternion.Lerp(lastRotation, targetRotation, t);
        }
    }
}