using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace IK {
    public class IKLink {
        public IKLink(Vector3 rotation, Vector3 position) {
            this.rotation = rotation;
            this.position = position;
        }

        public Vector3 rotation;
        public Vector3 position;
    }

    public class IKCCD {
        

        public static bool TwoLinkCCD(IKSegment baseLink, IKSegment endLink, IKTarget target) {
            int loops = 10;
            IKLink[] links = new IKLink[2];
            links[0] = new IKLink(endLink.transform.eulerAngles, endLink.transform.position);
            links[1] = new IKLink(baseLink.transform.eulerAngles, baseLink.transform.position);

            return true;
        }

        private IKLink point(IKLink link, IKTarget target) {
            return link;
        }
    }
}
