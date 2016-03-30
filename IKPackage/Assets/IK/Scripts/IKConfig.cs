using UnityEngine;
using System.Collections.Generic;
using System.Collections;
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
        List<IKSegment> LeftArm = new List<IKSegment>();
        void Awake() {
            LeftArm.Add(Left.Shoulder);
            LeftArm.Add(Left.UpperArm);
            LeftArm.Add(Left.LowerArm);
            LeftArm.Add(Left.Hand);

            ScheduleContact(false, Goal, 5f);
        }

        public void ScheduleContact(bool rightHand, IKTarget IKGoal, float duration) {
            if(IKCCD.CCD(LeftArm.ToArray(), IKGoal)) {
                float fun = 0f;
                foreach (IKSegment segment in LeftArm) {
                    StartCoroutine(ScheduleIK(segment, fun, duration));
                    fun += 0.5f;
                }
            }
        }

        IEnumerator ScheduleIK(IKSegment segment, float delay = 0f, float duration = 1f) {
            yield return new WaitForSeconds(delay);
            float elapsed = 0f;
            while(elapsed <= duration) {
                segment.RotateStep(elapsed / duration);
                yield return new WaitForEndOfFrame();
                elapsed += Time.deltaTime;
            }
        }
    }
}
