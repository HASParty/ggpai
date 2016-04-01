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
        public IKSegment UpperBack,
                         MiddleBack;

#if UNITY_EDITOR
        [System.Serializable]
        public struct TimingTest {
            public IKTarget goal;
            public float delay;
            public float reach;
            public float hold;
            public float retract;
        }

        public TimingTest[] goals;
#endif

        private IKSegment[] left, right;
        void Awake() {
            List<IKSegment> LeftArm = new List<IKSegment>();
            List<IKSegment> RightArm = new List<IKSegment>();
            addToChain(ref LeftArm, MiddleBack);
            addToChain(ref LeftArm, UpperBack);
            addToChain(ref LeftArm, Left.Shoulder);
            addToChain(ref LeftArm, Left.UpperArm);
            addToChain(ref LeftArm, Left.LowerArm);
            addToChain(ref LeftArm, Left.Hand);
            addToChain(ref RightArm, MiddleBack);
            addToChain(ref RightArm, UpperBack);
            addToChain(ref RightArm, Right.Shoulder);
            addToChain(ref RightArm, Right.UpperArm);
            addToChain(ref RightArm, Right.LowerArm);
            addToChain(ref RightArm, Right.Hand);
            left = LeftArm.ToArray();
            right = RightArm.ToArray();
#if UNITY_EDITOR
            Testing();
#endif
        }

#if UNITY_EDITOR
        void Testing() {
            foreach(TimingTest t in goals) {
                StartCoroutine(ScheduleDelayed(t));
            }
        }

        IEnumerator ScheduleDelayed(TimingTest item) {
            yield return new WaitForSeconds(item.delay);
            Debug.Log(item.goal.name);
            ScheduleContact(false, item.goal, item.reach, item.hold, item.retract);
        }
#endif

        private void addToChain(ref List<IKSegment> chain, IKSegment segment) {
            if (segment != null) chain.Add(segment);
        }

        public void ScheduleContact(bool rightHand, IKTarget IKGoal, float reach, float hold, float retract) {
            IKSegment[] chain = (rightHand ? right : left);
            if(IKCCD.CCD(chain, IKGoal)) {
                foreach (IKSegment segment in chain) {
                    StartCoroutine(ScheduleIK(segment, reach, hold, retract));
                }
            }
        }

        IEnumerator ScheduleIK(IKSegment segment, float reachDuration = 1f, float holdDuration = 1f, float revertDuration = 1f) {
            int process = segment.StartIK();
            Debug.Log("PROCESS " + process);
            float elapsed = 0f;
            float easeIn = (segment.EaseIn != 0 ? segment.EaseIn : 1f);
            float easeOut = segment.EaseOut;
            while (process == segment.CurrentIK() && elapsed <= reachDuration) {
                float t = IKMath.Catmull(elapsed / reachDuration, easeIn, 0, 1, easeOut);
                segment.RotateStep(t);
                yield return new WaitForEndOfFrame();
                elapsed += Time.deltaTime;
            }
            elapsed = 0f;
            while(process == segment.CurrentIK() && elapsed <= holdDuration) {
                segment.RotateStep(1f);
                yield return new WaitForEndOfFrame();
                elapsed += Time.deltaTime;
            }
            //yield return new WaitForSeconds(segment.LagFactor * revertDuration);
            elapsed = 0f;
            while (process == segment.CurrentIK() && elapsed <= revertDuration) {                
                float t = IKMath.Catmull(elapsed / revertDuration, easeIn, 0, 1, easeOut);
                segment.RotateStep(1-t, reverting: true);
                yield return new WaitForEndOfFrame();
                elapsed += Time.deltaTime;
            }

            Debug.LogFormat("{0} {1}", process, segment.CurrentIK());

        }
    }
}
