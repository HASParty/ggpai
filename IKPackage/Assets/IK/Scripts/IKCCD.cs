using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace IK {
    public class IKLink {
        public IKLink(IKSegment copy) {
            GameObject go = new GameObject();
            transform = go.transform;
            go.name = "Spoof"+copy.name;
            //not sure if it copies or is a reference
            transform.rotation = copy.transform.rotation;
            transform.position = copy.transform.position;
            //I hope this works
            transform.localScale = copy.transform.localScale;
            segRef = copy;
        }

        public Transform transform;
        public IKSegment segRef;
    }

    public class IKCCD {

        private static IKLink[] createLinks(IKSegment[] segments) {
            IKLink[] links = new IKLink[segments.Length];
            IKLink previous = null;
            for(int i = 0; i < links.Length; i++) {
                links[i] = new IKLink(segments[i]);
                if(previous != null) {
                    links[i].transform.SetParent(previous.transform);
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

        private static void constrainLink(IKLink link) {
            if (!link.segRef.Constrain) return;
            Vector3 expected = link.transform.rotation.eulerAngles;
            Vector3 baseRot = link.segRef.GetBaseRotation().eulerAngles;
            Vector3 min = baseRot + link.segRef.RotationConstraintsMin;
            Vector3 max = baseRot + link.segRef.RotationConstraintsMax;
            expected.x = Mathf.Clamp(expected.x, min.x, max.x);
            expected.y = Mathf.Clamp(expected.y, min.y, max.y);
            expected.z = Mathf.Clamp(expected.z, min.z, max.z);
            link.transform.localRotation = Quaternion.Euler(expected.x, expected.y, expected.z);
        }

        public static bool CCD(IKSegment[] segments, IKTarget target) {
            var links = createLinks(segments);
            if (links.Length < 2) return false;
            IKLink end = links[links.Length - 1];

            int link = links.Length - 1;
            bool success = false;

            for(int i = 0; i < links.Length*30; i++) { 
                IKLink root = links[link];
                if (Vector3.Distance(end.transform.position, target.transform.position) > 0.05f) {
                    Vector3 currentVector = (end.transform.position - root.transform.position).normalized;
                    Vector3 targetVector = (target.transform.position - root.transform.position).normalized;

                    float cosAngle = Vector3.Dot(targetVector, currentVector);

                    if (cosAngle < 0.9999f) {
                        Vector3 cross = Vector3.Cross(currentVector, targetVector).normalized;
                        float turn = Mathf.Min(Mathf.Rad2Deg * Mathf.Acos(cosAngle), root.segRef.DampDegrees);
                       
                        Quaternion result = Quaternion.AngleAxis(turn, cross);
                        root.transform.rotation = result*root.transform.rotation;
                        //constrainLink(links[link]);
                    }

                    link--;

                    if (link < 0) link = links.Length - 1;
                } else {
                    success = true;
                }
            }

           if (success) {
                for (int i = 0; i < links.Length; i++) {
                    segments[i].SetTargetRotation(links[i].transform.localRotation);
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
