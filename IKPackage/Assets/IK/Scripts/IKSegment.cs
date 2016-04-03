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
        public Vector3 x, y;

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
            IKInProcess++;
            return IKInProcess;
        }

        public int CurrentIK() {
            return IKInProcess;
        }

        public void Grab(IKTarget target) {
            //TODO: properly
            target.transform.SetParent(transform);
        }

        public void SetTargetRotation(Quaternion newTarget) {
            targetRotation = newTarget;
            lastRotation = transform.localRotation;
        }

        public void RotateStep(float t, bool reverting = false) {
            Quaternion from = lastRotation;
            Quaternion to = targetRotation;
            if (reverting) {
                //if reverting interpolate back into animation pose
                from = originalLocalRotation;
            }
            transform.localRotation = Quaternion.Lerp(from, to, t);
            
        }
    }
}