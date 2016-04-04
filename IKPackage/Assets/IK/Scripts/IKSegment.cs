using UnityEngine;
using System.Collections;
namespace IK {
    public enum IKJointType {
        Cone, //shoulder, thumb base
        OneDOF, //fingers
        TwoDOF //finger base
    }
        

    public class IKSegment : MonoBehaviour {
        public IKJointType JointType;

        public float ConeRadius;
        public Vector3 Min, Max;
        public bool Twist;

        public Vector3 originalDir;
        public Transform End;


        public float DampDegrees;
        public bool Constrain = true;

        public float EaseIn;
        public float EaseOut;
        public float Bounce;


        private int IKInProcess = -1;


        private Quaternion originalRotation;
        private Quaternion originalLocalRotation;
        private Quaternion lastRotation;
        private Quaternion targetRotation;

        void Awake() {
            Init();
        }

        public void Init() {
            originalRotation = transform.rotation;
            originalLocalRotation = transform.localRotation;
            if(End == null && JointType != IKJointType.Cone) {
                if(transform.childCount > 0) {
                    Debug.LogWarning("Automatically configured end point of " + name);
                    End = transform.GetChild(0);
                } else {
                    Debug.LogError("Missing end of " + name);
                }
            }
            if (End != null) {
                originalDir = (Quaternion.Inverse(transform.parent.rotation) * (End.position - transform.position)).normalized;
            } else if (JointType != IKJointType.Cone) {
                Debug.LogErrorFormat("Could not configure {0} joint.", name);
            }
        }

        public Quaternion GetBaseRotation() {
            return originalRotation;
        }

        public int StartIK() {
            ik = true;
            IKInProcess++;
            return IKInProcess;
        }

        public void StopIK() {
            ik = false;
        }

        public int CurrentIK() {
            return IKInProcess;
        }

#if UNITY_EDITOR
        float currentTwist = 0;
        int dirTwist = 1;
        bool testingTwist = false;
        public void TestTwist() {
            testingTwist = !testingTwist;
        }

        float currentX = 0;
        int dirX = 1;
        bool testingX = false;
        public void TestX() {
            testingX = !testingX;
        }

        float currentY = 0;
        int dirY = 1;
        bool testingY = false;
        public void TestY() {
            testingY = !testingY;
        }
#endif
        void LateUpdate() {
#if UNITY_EDITOR
           
            float step = 60 * Time.deltaTime;
            if (testingTwist || testingX || testingY) currentRot = originalLocalRotation;

            Vector3 toChild = transform.parent.rotation * originalDir;
            if (testingX) {
                Vector3 xAxis = transform.parent.up;
                if (currentX >= Max.x) dirX = -1;
                else if (currentX <= Min.x) dirX = 1;
                currentX += dirX * step;              
                currentRot *= Quaternion.AngleAxis(currentX, xAxis);
                Debug.LogFormat("Actual {0} calculated {1}", currentX, IKMath.AngleOnPlane(currentRot * toChild, toChild, xAxis));
            }

            if (testingY) {
                Vector3 yAxis = transform.parent.right;
                if (currentY >= Max.y) dirY = -1;
                else if (currentY <= Min.y) dirY = 1;
                currentY += dirY * step;
                currentRot *= Quaternion.AngleAxis(currentY, yAxis);
            }

            if (testingTwist) {
                Debug.Log("TWISTY");
                Vector3 twistAxis = transform.right;
                if (currentTwist >= Max.z) dirTwist = -1;
                else if (currentTwist <= Min.z) dirTwist = 1;
                currentTwist += dirTwist * step;
                Debug.Log(currentTwist);
                currentRot *= Quaternion.AngleAxis(currentTwist, Vector3.right);
            }

            

            if (ik || testingX || testingY || testingTwist) {
#else
            if (ik) {
#endif
                transform.localRotation = currentRot;
            }
        }

        public void Grab(IKTarget target) {
            //TODO: properly
            target.transform.SetParent(transform);
        }

        public void SetTargetRotation(Quaternion newTarget) {
            targetRotation = newTarget;
            lastRotation = transform.localRotation;
        }

        bool ik = false;
        Quaternion currentRot;
        public void RotateStep(float t, bool reverting = false) {
            Quaternion from = lastRotation;
            Quaternion to = targetRotation;
            if (reverting) {
                //if reverting interpolate back into animation pose
                from = originalLocalRotation;
            }
            currentRot = Quaternion.Slerp(from, to, t);
            
        }
    }
}