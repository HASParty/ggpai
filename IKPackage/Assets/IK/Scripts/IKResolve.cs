using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace IK {
    public class IKLink {
        public IKLink(IKSegment reference) {
            GameObject go = new GameObject();
            transform = go.transform;
            go.name = "Spoof"+ reference.name;
            go.transform.localScale = reference.transform.localScale;
            go.transform.rotation = reference.transform.rotation;
            go.transform.position = reference.transform.position;
            segRef = reference;
        }

        public Transform transform;
        public Transform hingeAbout;
        public IKSegment segRef;
    }

    public class IKResolve {

        private static void connectTransform(Transform from, Transform to, Transform connectParent, Transform connectChild) {
            Transform prev = connectChild;
            Transform curr = to.parent;
            while (from != curr) {
               // Debug.Log(curr.name);
                GameObject go = new GameObject();
                go.name = "Spoof" + curr.name;
                go.transform.SetParent(prev);
                go.transform.localScale = curr.transform.localScale;
                go.transform.rotation = curr.transform.rotation;
                go.transform.position = curr.transform.position;
                prev = go.transform;
                curr = curr.parent;
            }
            connectParent.SetParent(prev);
        }

        private static IKLink[] createLinks(IKSegment[] segments, Transform effector, out Transform contact) {
            IKLink[] links = new IKLink[segments.Length];
            IKLink previous = null;
            for(int i = 0; i < links.Length; i++) {
                links[i] = new IKLink(segments[i]);
                if(previous != null) {
                    //Debug.LogFormat("from {0} to {1}", previous.transform.name, links[i].transform.name);
                    connectTransform(previous.segRef.transform, links[i].segRef.transform, links[i].transform, previous.transform);
                } else {
                    links[i].transform.SetParent(segments[0].transform.parent);
                }
                previous = links[i];
            }

            GameObject go = new GameObject();
            go.name = "Effector";
            go.transform.SetParent(links[links.Length - 1].transform);
            go.transform.position = effector.position;
            go.transform.rotation = effector.rotation;
            go.transform.localScale = effector.localScale;
            contact = go.transform;

            return links;
        }

        private static void destroyLinks(IKLink[] links) {
            if(links.Length > 0) GameObject.Destroy(links[0].transform.gameObject);
        }

        private static Quaternion constrainCone(Quaternion quat, IKLink link) {
           // Debug.Log(link.transform.name);
            if (!link.segRef.Constrain) return quat;
            Quaternion baseQuat = link.segRef.GetBaseRotation();
            float error = 360;
            int iterations = 1;
            while (error > 0.5f && iterations < 10) {
                Vector3 center = baseQuat * Vector3.up;
                Vector3 expected = quat * Vector3.up;

                float angle = Vector3.Angle(center, expected);
               // Debug.LogFormat("{0} {1}", link.segRef.ConeRadius, angle);
                if(angle > link.segRef.ConeRadius) {
                    quat = Quaternion.Slerp(Quaternion.identity, quat, 1 - iterations*0.1f);
                } else {
                    //Debug.Log("DONE");
                    break;
                }

                iterations++;
            }
            return quat;
        }

        private static Quaternion constrainAxis(IKLink root, Vector3 axis, Vector3 angleAxis, Vector3 toChild, Vector3 dir, float min, float max, out float actualAngle) {
            actualAngle = IKMath.AngleOnPlane(dir, toChild, axis);

            float angle = Mathf.Clamp(actualAngle, min, max);
            //Debug.LogFormat("{2}: {0} {1}", actualAngle, angle, root.transform.name);
            return Quaternion.AngleAxis(angle, angleAxis);
        }

        public static bool CCD(IKSegment[] segments, IKEndEffector effector, IKTarget target, out Quaternion[] goals) {
            goals = new Quaternion[segments.Length];
            Transform contact;
            var links = createLinks(segments, effector.transform, out contact);
            if (links.Length < 2) return false;
            IKLink end = links[links.Length - 1];
            end.transform.localRotation = target.transform.rotation;
            int link = links.Length - 2;
            bool success = false;
            for(int i = 0; i < links.Length*30; i++) {
                IKLink root = links[link];
                if (Vector3.Distance(contact.position, target.transform.position) > target.Radius) {
                    Vector3 currentVector = (contact.position - root.transform.position).normalized;
                    Vector3 targetVector = (target.transform.position - root.transform.position).normalized;

                    float cosAngle = Vector3.Dot(targetVector, currentVector);

                    if (cosAngle < 0.9999f) {
                        Vector3 cross = Vector3.Cross(currentVector, targetVector).normalized;
                        float turn = Mathf.Min(Mathf.Rad2Deg * Mathf.Acos(cosAngle), root.segRef.DampDegrees);
                        Quaternion result = Quaternion.AngleAxis(turn, cross);
                        //Constrain the joint if it is to be constrained
                        if (root.segRef.Constrain) {
                            if (root.segRef.JointType == IKJointType.Cone) {
                                result = constrainCone(result, root);
                            } else {
                                Vector3 toChild = root.transform.parent.rotation * root.segRef.originalDir;
                                Vector3 dir = (result * toChild).normalized;
                                Vector3 xAxis = root.segRef.ParentOffset*root.transform.parent.up;
                                float actualAngle;
                                //Debug.Log("Constraining x");
                                Quaternion xConstrain = constrainAxis(root, xAxis, Vector3.up, toChild, dir, root.segRef.Min.x, root.segRef.Max.x, out actualAngle);
                                if (root.segRef.JointType == IKJointType.TwoDOF) {
                                    Vector3 dirMinusXAxis = Quaternion.Inverse(Quaternion.AngleAxis(actualAngle, Vector3.up)) * dir;
                                    Vector3 yAxis = root.segRef.ParentOffset*root.transform.parent.forward;
                                    //Debug.Log("Constraining y");
                                    Quaternion yConstrain = constrainAxis(root, yAxis, Vector3.forward, toChild, dirMinusXAxis, root.segRef.Min.y, root.segRef.Max.y, out actualAngle);
                                    result = xConstrain * yConstrain;
                                } else {
                                    result = xConstrain;
                                }
                            }

                            if (root.segRef.Twist) {
                               // Debug.Log("Constraining twist");
                                Vector3 toChild = root.transform.parent.rotation * root.segRef.originalDir;
                                Vector3 dir = (result * toChild).normalized;
                                float actualAngle;
                                //get legal twist
                                Quaternion zRot = constrainAxis(root, Vector3.right, Vector3.right, toChild, dir, root.segRef.Min.z, root.segRef.Max.z, out actualAngle);
                                //untwist by actual twist and reapply with legal twist
                                result = Quaternion.Inverse(Quaternion.AngleAxis(actualAngle, Vector3.right)) * result;
                                result = zRot * result;
                            }
                        }
                        root.transform.rotation = result*root.transform.rotation;
                    }

                    link--;

                    if (link < 0) link = links.Length - 2;
                } else {
                    success = true;
                }
           }
            if (success) {
                for (int i = 0; i < links.Length; i++) {
                    goals[i] = links[i].transform.localRotation;
                }
            }

            destroyLinks(links);

            return success;
        }

        private IKLink point(IKLink link, IKTarget target) {
            return link;
        }
    }
}
