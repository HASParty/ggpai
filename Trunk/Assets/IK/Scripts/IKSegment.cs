using UnityEngine;
using System.Collections;
namespace IK {
    public class IKSegment : MonoBehaviour {
        public Vector3 JointBase;
        public Vector3 JointEnd;
        public Vector3 RotationConstraintsUpper;
        public Vector3 RotationConstraintsLower;
        private Vector3 originalRotation;
        public Vector3 OriginalPosition;

        // Use this for initialization
        void Awake() {
            Init();
        }

        public void Init() {
            originalRotation = transform.localEulerAngles;
            OriginalPosition = transform.position;
        }

        void LateUpdate() {            

        }

        //honestly just testing purposes,
        //the resolver should take care to observe and preserve
        //constraints
        public void Rotate(Vector3 euler) {
            Vector3 newRotation;
            transform.RotateAround(JointBase + OriginalPosition, Vector3.right, euler.x);
            transform.RotateAround(JointBase + OriginalPosition, Vector3.up, euler.y);
            transform.RotateAround(JointBase + OriginalPosition, Vector3.forward, euler.z);
            newRotation = transform.localEulerAngles;
            //TODO: clamp to constraints
        }
    }
}