using UnityEngine;
using System.Collections;
namespace IK {
    public class IKSegment : MonoBehaviour {
        public Vector3 RotationConstraintsMax;
        public Vector3 RotationConstraintsMin;
        public float DampDegrees;
        public bool Constrain = true;

        [Header("Timing")]
        public float EaseIn;
        public float EaseOut;
        public float Bounce;

        private int IKInProcess = -1;

        

        private Quaternion originalRotation;
        private Quaternion lastRotation;
        private Quaternion targetRotation;

        void Awake() {
            originalRotation = transform.localRotation;
        }

        public Quaternion GetBaseRotation() {
            return originalRotation;
        }

        public int StartIK() {
            IKInProcess++;
            return IKInProcess;
        }

        public int CurrentIK() {
            return IKInProcess;
        }

        public void Grab(IKTarget target) {
            //TODO: properly
            target.transform.SetParent(transform);
        }

        public void SetTargetRotation(Quaternion newTarget) {
            targetRotation = newTarget;
            lastRotation = transform.localRotation;
        }

        public void RotateStep(float t, bool reverting = false) {
            Quaternion from = lastRotation;
            Quaternion to = targetRotation;
            if (reverting) {
                //if reverting interpolate back into animation pose
                from = originalRotation;
            }
            transform.localRotation = Quaternion.Lerp(from, to, t);
            
        }
    }
}