using UnityEngine;
using System.Collections.Generic;

namespace IK {
    public enum IKHandShape {
        ThreePointContact, //GO shape
        SimpleFingerGrab,
        HandWrap
    }

    public class IKTarget : MonoBehaviour {
        public IKHandShape HandShape;
        public List<IKContactPoint> ContactPoints;
    }
}
