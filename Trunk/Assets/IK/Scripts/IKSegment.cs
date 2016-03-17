using UnityEngine;
using System.Collections;
namespace IK {
    public class IKSegment : MonoBehaviour {
        public string ID;
        public Vector3 RotationConstraintsUpper;
        public Vector3 RotationConstraintsLower;

        void Awake() {
        }
    }
}