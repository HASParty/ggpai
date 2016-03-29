using UnityEngine;
using System.Collections.Generic;
namespace IK {
    [System.Serializable]
    public struct IKSegmentPair {
        public string ID;
        public IKSegment Upper;
        public IKSegment Lower;
        public float Damping;
        public IKSegmentType Type;
        public IKTarget Target;
        public Quaternion TargetOrientationUpper;
        public Quaternion TargetOrientationLower;
    }

    public enum IKSegmentType {
        Head,
        UpperBack,
        LowerBack,
        LeftArm,
        LeftIndexFinger,
        LeftMiddleFinger,
        LeftRingFinger,
        LeftThumb,
        RightArm,
        RightIndexFinger,
        RightMiddleFinger,
        RightRingFinger,
        RightThumb
    }

    public class IKConfig : MonoBehaviour {
        public List<IKSegmentPair> Pairs;
        private Dictionary<string, IKSegmentPair> pairLookup;

        public IKTarget Goal;
         
        [Header("General Config")]
        public bool OverrideHeadLook;

        void Awake() {
            pairLookup = new Dictionary<string, IKSegmentPair>();
            foreach(var pair in Pairs) {
                pairLookup.Add(pair.ID, pair);
            }

            Debug.Log(IKCCD.TwoLinkCCD(Pairs[0].Upper, Pairs[0].Lower, Goal));
        }

        public void ScheduleContact(bool rightHand, IKTarget IKGoal, float duration) {
            //TODO: resolve for all relevant segments that are available
        }

        void ScheduleIK(string segmentID, Quaternion rotationGoal, float delay = 0f, float duration = 1f) {
            //helper routine
        }

    }
}
