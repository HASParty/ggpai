using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace IK {
    public class IKLink {
        public IKLink(Transform copy, Quaternion constraints) {
            GameObject go = new GameObject();
            transform = go.transform;
            //not sure if it copies or is a reference
            transform.rotation = copy.rotation;
            transform.position = copy.position;
            //I hope this works
            transform.localScale = copy.localScale;
            this.constraints = constraints;   
        }

        public Transform transform;
        public Quaternion constraints;
    }

    public class IKCCD {
        
        private static IKLink[] createLinks(IKSegment[] segments) {
            IKLink[] links = new IKLink[segments.Length];
            IKLink previous = null;
            for(int i = 0; i < links.Length; i++) {
                links[i] = new IKLink(segments[i].transform, segments[i].GetConstraints());
                if(previous != null) {
                    links[i].transform.SetParent(previous.transform);
                }
            }

            return links;
        }

        private static void destroyLinks(IKLink[] links) {
            if(links.Length > 0) GameObject.Destroy(links[0].transform.gameObject);
        }

        public static bool CCD(IKSegment[] segments, IKTarget target) {
            var links = createLinks(segments);
            if (links.Length < 2) return false;
            IKLink end = links[links.Length - 1];

            int link = links.Length - 1;
            bool success = false;

            for(int i = 0; i < links.Length; i++) { 
                Vector3 root = links[link].transform.position;

                if (Vector3.Distance(end.transform.position, target.transform.position) > 0.05f) {
                    Vector3 currentVector = end.transform.position - root;
                    Vector3 targetVector = target.transform.position - root;

                    currentVector.Normalize();
                    targetVector.Normalize();

                    float cosAngle = Vector3.Dot(targetVector, currentVector);

                    if (cosAngle < 1f) {
                        Vector3 cross = Vector3.Cross(currentVector, targetVector).normalized;
                        float turn = Mathf.Acos(cosAngle);
                        //TODO: damp turn if damping
                        Quaternion result = Quaternion.AngleAxis(turn, cross);
                        links[link].transform.rotation = links[link].transform.rotation * result;
                        //TODO: DOF restrictions
                    }

                    link--;

                    if (link < 0) link = links.Length - 1;
                } else {
                    success = true;
                }
            }

            //if (success) {
            //    Debug.Log("YAY");
                for (int i = 0; i < links.Length; i++) {
                    segments[i].SetTargetRotation(links[i].transform.rotation);
                }
            //}

            Debug.Log("Donezo");

            return success;
        }

        private IKLink point(IKLink link, IKTarget target) {
            return link;
        }
    }
}
