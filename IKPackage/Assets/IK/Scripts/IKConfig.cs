using UnityEngine;
using System.Collections.Generic;
namespace IK {

    public class IKConfig : MonoBehaviour {
        [System.Serializable]
        public struct Arm {
            public IKSegment Shoulder, 
                             UpperArm,
                             LowerArm,
                             Hand,
                             IndexFinger,
                             MiddleFinger,
                             RingFinger,
                             Thumb;
        }

        public Arm Left;
        public Arm Right;
        public IKSegment Head,
                         Neck,
                         UpperBack,
                         MiddleBack;

        public IKTarget Goal;
         
        [Header("General Config")]
        //if using headlookcontroller tries to override, just uncheck
        //or make sure headlookcontroller executes after
        public bool AffectHead;
        List<IKSegment> segs = new List<IKSegment>();
        void Awake() {            
            segs.Add(Left.Shoulder);
            segs.Add(Left.UpperArm);
            segs.Add(Left.LowerArm);
            segs.Add(Left.Hand);
        }

        Vector3 lastPos = Vector3.zero;
        void Update() {
            if(lastPos != Goal.transform.position) {
                IKCCD.CCD(segs.ToArray(), Goal);
                lastPos = Goal.transform.position;
            }
            
        }

        public void ScheduleContact(bool rightHand, IKTarget IKGoal, float duration) {
            //TODO: resolve for all relevant segments that are available
        }

        void ScheduleIK(string segmentID, Quaternion rotationGoal, float delay = 0f, float duration = 1f) {
            //helper routine
        }

    }
}
