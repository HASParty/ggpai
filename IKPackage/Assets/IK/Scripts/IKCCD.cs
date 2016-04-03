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
        public IKSegment segRef;
    }

    public class IKCCD {

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

        private static IKLink[] createLinks(IKSegment[] segments) {
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

            return links;
        }

        private static void destroyLinks(IKLink[] links) {
            if(links.Length > 0) GameObject.Destroy(links[0].transform.gameObject);
        }

        private static Quaternion constrainLink(Quaternion quat, IKLink link) {
            Debug.Log(link.transform.name);
            if (!link.segRef.Constrain) return quat;
            Vector3 min = link.segRef.Min;
            Vector3 max = link.segRef.Max;
            Quaternion baseQuat = link.segRef.GetBaseRotation();
            float error = 360;
            int iterations = 1;
            while (error > 0.5f && iterations < 10) {
                Vector3 center = baseQuat * Vector3.up;
                Vector3 expected = quat * Vector3.up;

                float angle = Vector3.Angle(center, expected);
                //Debug.LogFormat("{0} {1}", link.segRef.ConeRadius, angle);
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

        public static bool CCD(IKSegment[] segments, IKTarget target, out Quaternion[] goals) {
            goals = new Quaternion[segments.Length];
            var links = createLinks(segments);
            if (links.Length < 2) return false;
            IKLink end = links[links.Length - 1];
            end.transform.localRotation = target.transform.localRotation;
            int link = links.Length - 2;
            bool success = false;
            for(int i = 0; i < links.Length*30; i++) {
                IKLink root = links[link];
                if (Vector3.Distance(end.transform.position, target.transform.position) > target.Radius) {
                    Vector3 currentVector = (end.transform.position - root.transform.position).normalized;
                    Vector3 targetVector = (target.transform.position - root.transform.position).normalized;

                    float cosAngle = Vector3.Dot(targetVector, currentVector);

                    if (cosAngle < 0.9999f) {
                        Vector3 cross = Vector3.Cross(currentVector, targetVector).normalized;
                        float turn = Mathf.Min(Mathf.Rad2Deg * Mathf.Acos(cosAngle), root.segRef.DampDegrees);
                        Quaternion result = Quaternion.AngleAxis(turn, cross);
                        if (root.segRef.JointType == IKJointType.Cone) {
                            result = constrainLink(result, root);
                        } else {
                            Vector3 toChild = root.segRef.originalDir;
                            Vector3 axis = (root.transform.rotation*root.segRef.x).normalized;
                            Vector3 dir = (result * toChild).normalized;
                            Vector3 projection = Vector3.ProjectOnPlane(dir, axis);
                            cosAngle = Vector3.Dot(toChild, projection);
                            Vector3 norm = Vector3.Cross(toChild, projection);
                            cosAngle = cosAngle*Mathf.Sign(Vector3.Dot(axis, norm));
                           
                            float actualAngle = Mathf.Rad2Deg * Mathf.Acos(cosAngle);

                            float angle = Mathf.Clamp(actualAngle, root.segRef.Min.x, root.segRef.Max.x);
                            Debug.LogFormat("{2}: {0} {1}", actualAngle, angle, root.transform.name);
                            result = Quaternion.AngleAxis(angle, axis);                                                       
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
