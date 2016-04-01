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
            Vector3 min = link.segRef.RotationConstraintsMin;
            Vector3 max = link.segRef.RotationConstraintsMax;
            Quaternion baseQuat = link.segRef.GetBaseRotation();
            float error = 360;
            int brbr = 0;
            while (error > 5f && brbr < 5) {
                brbr++;
                Vector3 center = baseQuat * Vector3.up;
                Vector3 expected = quat * Vector3.up;
                float y = Vector3.Angle(center, expected);
                expected = quat * Vector3.right;
                center = baseQuat * Vector3.right;
                float x = Vector3.Angle(center, expected);
                expected = quat * Vector3.forward;
                center = baseQuat * Vector3.forward;
                float z = Vector3.Angle(center, expected);
                if (y > max.y || y < min.y || x > max.x || x < min.x || z > max.z || z < min.z) {
                    Debug.Log("ER OR PR");
                    if (y > max.y) error = Mathf.Abs(max.y - y);
                    else error = Mathf.Abs(y - min.y);
                    if (x > max.x) error = Mathf.Max(error, Mathf.Abs(max.x - x));
                    else error = Mathf.Max(error, Mathf.Abs(x - min.x));
                    if (z > max.z) error = Mathf.Max(error, Mathf.Abs(max.z - z));
                    else error = Mathf.Max(error, Math.Abs(z - min.z));
                    quat = Quaternion.Slerp(baseQuat, quat, 0.75f);
                    Debug.LogFormat("{0} {1}", center, expected);
                    Debug.LogFormat("{0} {1} {2} {3} {4} {5}", max.x-x, min.x-x, max.y-y, min.y-y, max.z-z, min.z-z);
                } else {
                    Debug.Log("wagtagag");
                    break;
                }
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
                        result = constrainLink(result, links[link]);
                        root.transform.rotation = result*root.transform.rotation;
                    }

                    link--;

                    if (link < 0) link = links.Length - 1;
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
