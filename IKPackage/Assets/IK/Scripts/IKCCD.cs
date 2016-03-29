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
        
        private IKLink[] createLinks(IKSegment[] segments) {
            IKLink[] links = new IKLink[segments.Length];
            IKLink previous = null;
            foreach(IKSegment s in segments) {
                IKLink link = new IKLink(s.transform, s.GetConstraints());
                if(previous != null) {
                    link.transform.SetParent(previous.transform);
                }
            }

            return links;
        }

        private void destroyLinks(IKLink[] links) {
            if(links.Length > 0) GameObject.Destroy(links[0].transform.gameObject);
        }

        public static bool CCD(IKSegment[] segments, IKTarget target) {
            return true;
        }

        private IKLink point(IKLink link, IKTarget target) {
            return link;
        }
    }
}
