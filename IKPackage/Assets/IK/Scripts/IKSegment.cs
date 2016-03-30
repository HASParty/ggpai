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
            originalRotation = transform.localRotation;
        }

        float elapsed = 0f;
        float duration = 0f; 
        void Update() {
            elapsed += Time.deltaTime;
            if(elapsed <= duration) RotateStep(elapsed / duration);
        }

        public Vector3 GetBaseEuler() {
            return originalRotation.eulerAngles;
        }

        public void SetTargetRotation(Quaternion newTarget) {
            targetRotation = newTarget;
            lastRotation = transform.rotation;
            elapsed = 0f;
            duration = 1f;
            //Debug.LogFormat("wanna rot {0} from {1}", targetRotation.eulerAngles, lastRotation.eulerAngles);
        }

        public void RotateStep(float t) {
            transform.rotation = Quaternion.Lerp(lastRotation, targetRotation, t);
        }
    }
}