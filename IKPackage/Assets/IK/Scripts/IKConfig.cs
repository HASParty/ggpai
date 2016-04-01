using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
namespace IK {

    public class IKConfig : MonoBehaviour {
        [System.Serializable]
        public struct Arm {
            public IKSegment Shoulder,
                             UpperArm,
                             LowerArm,
                             Hand;
            public Finger    IndexFinger,
                             MiddleFinger,
                             RingFinger,
                             PinkyFinger,
                             Thumb;
        }
        [System.Serializable]
        public struct Finger {
            public IKSegment Base,
                             Mid,
                             End;
        }

        public Arm Left;
        public Arm Right;
        public IKSegment UpperBack,
                         MiddleBack;

        private IKSegment[] left, right;
        private Dictionary<IKContactPoint.Fingers, IKSegment[]> leftHand;
        private Dictionary<IKContactPoint.Fingers, IKSegment[]> rightHand;
        void Awake() {
            left = makeArm(Left);
            right = makeArm(Right);

            leftHand = makeHand(Left);
            rightHand = makeHand(Right);

            ScheduleContact(false, test, 5f);
        }

        private IKSegment[] makeArm(Arm arm) {
            List<IKSegment> chain = new List<IKSegment>();
            addToChain(ref chain, MiddleBack);
            addToChain(ref chain, UpperBack);
            addToChain(ref chain, arm.Shoulder);
            addToChain(ref chain, arm.UpperArm);
            addToChain(ref chain, arm.LowerArm);
            addToChain(ref chain, arm.Hand);
            return chain.ToArray();
        }

        private Dictionary<IKContactPoint.Fingers, IKSegment[]> makeHand(Arm arm) {
            var hand = new Dictionary<IKContactPoint.Fingers, IKSegment[]>();
            hand.Add(IKContactPoint.Fingers.Index, makeFinger(arm.IndexFinger));
            hand.Add(IKContactPoint.Fingers.Middle, makeFinger(arm.MiddleFinger));
            hand.Add(IKContactPoint.Fingers.Ring, makeFinger(arm.RingFinger));
            hand.Add(IKContactPoint.Fingers.Pinky, makeFinger(arm.PinkyFinger));
            return hand;
        }

        private IKSegment[] makeFinger(Finger finger) {
            List<IKSegment> chain = new List<IKSegment>();
            addToChain(ref chain, finger.Base);
            addToChain(ref chain, finger.Mid);
            addToChain(ref chain, finger.End);
            return chain.ToArray();
        }

        private void addToChain(ref List<IKSegment> chain, IKSegment segment) {
            if (segment != null) chain.Add(segment);
        }

        public IKTiming[] test;

        //TODO: make generating the timings somewhat automated
        public void ScheduleContact(bool rightHand, IKTiming[] timings, float duration) {
            IKSegment[] chain = (rightHand ? right : left);
            //just to be sure things are in the right order
            timings = timings.OrderBy(x => x.Timing).ToArray();

            List<Quaternion[]> goals = new List<Quaternion[]>();
            //calculate orientations
            for(int i = 0; i < timings.Length; i++) {
                Quaternion[] result;
                if (IKCCD.CCD(chain, timings[i].Target, out result)) {
                    goals.Add(result);
                } else {
                    Debug.LogWarning("Unreachable goal");
                    return;
                }
            }

            UnityEngine.Assertions.Assert.IsTrue(goals.Count == timings.Length);

            //schedule
            StartCoroutine(ScheduleIK(chain, timings, goals, duration));

        }

        IEnumerator ScheduleIK(IKSegment[] chain, IKTiming[] timings, List<Quaternion[]> rotations, float duration) {
            float lastTiming = 0;
            for(int i = 0; i < timings.Length; i++) {
                float revert = 0f;
                float reach = (timings[i].Timing - lastTiming) * duration;

                if (i == timings.Length - 1) {
                    revert = (1 - timings[i].Timing) * duration;
                }

                for (int seg = 0; seg < chain.Length; seg++) {                    
                    chain[seg].SetTargetRotation(rotations[i][seg]);
                    StartCoroutine(ScheduleIK(chain[seg], reach, 0, revert, 0));
                }

                lastTiming = timings[i].Timing;
                yield return new WaitForSeconds(reach);
                if(timings[i].GrabTarget) {
                    chain[chain.Length - 1].Grab(timings[i].Target);
                }
            }
        }

        IEnumerator ScheduleIK(IKSegment segment, float reachDuration = 1f, float holdDuration = 1f, float revertDuration = 1f, float delay = 0f) {
            yield return new WaitForSeconds(delay);
            int process = segment.StartIK();
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
            if (revertDuration > 0) {
                while (process == segment.CurrentIK() && elapsed <= revertDuration) {
                    float t = IKMath.Catmull(elapsed / revertDuration, easeIn, 0, 1, easeOut);
                    segment.RotateStep(1 - t, reverting: true);
                    yield return new WaitForEndOfFrame();
                    elapsed += Time.deltaTime;
                }
            }

        }
    }
}
