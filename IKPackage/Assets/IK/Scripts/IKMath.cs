using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace IK {
    public class IKMath {
        /// <summary>
        /// Easing in and out quadratically
        /// </summary>
        /// <param name="t">Current time</param>
        /// <param name="b">Start value</param>
        /// <param name="c">Change in value</param>
        /// <param name="d">Duration</param>
        /// <returns>t to be used in lerp</returns>
        public static float EaseInOutQuadratic(float t, float b, float c, float d) {
            t /= d / 2;
            if (t < 1) return c / 2 * t * t + b;
            t--;
            return -c / 2 * (t * (t - 2) - 1) + b;
        }

        /// <summary>
        /// http://sol.gfxile.net/interpolation/
        /// Catmull spline function with four points
        /// </summary>
        /// <param name="t">time/duration</param>
        /// <param name="p0">start</param>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3">end</param>
        /// <returns>t to be used in lerp</returns>
        public static float Catmull(float t, float p0, float p1, float p2, float p3) {
            return 0.5f * (
                          (2 * p1) +
                          (-p0 + p2) * t +
                          (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t +
                          (-p0 + 3 * p1 - 3 * p2 + p3) * t * t * t
                          );
        }

        public static float AngleOnPlane(Vector3 a, Vector3 b, Vector3 normal) {
            a = Vector3.ProjectOnPlane(a, normal);
            b = Vector3.ProjectOnPlane(b, normal);
            float cosAngle = Vector3.Dot(a, b);
            Vector3 norm = Vector3.Cross(a, b);

            return Mathf.Rad2Deg * Mathf.Acos(cosAngle) * -Mathf.Sign(Vector3.Dot(normal, norm));
        }

        public static Quaternion GetTwist(Vector3 around, Quaternion rotation) {
            Vector3 ra = new Vector3(rotation.x, rotation.y, rotation.z);
            Vector3 p = Vector3.Project(ra, around);
            Quaternion q = new Quaternion(p.x, p.y, p.z, rotation.w);
            NormaliseQuat(ref q);
            return q;
        }

        public static Quaternion GetSwing(Quaternion rotation, Quaternion twist) {
            Quaternion swing = rotation * ConjugatedQuat(twist);
            return swing;
        }

        public static void NormaliseQuat(ref Quaternion q) {
            float norm = Mathf.Sqrt(Mathf.Pow(q.x, 2) + Mathf.Pow(q.y, 2) + Mathf.Pow(q.z, 2) + Mathf.Pow(q.w, 2));
            q.Set(q.x / norm, q.y / norm, q.z / norm, q.w / norm);
        }

        public static Quaternion ConjugatedQuat(Quaternion q) {
            return new Quaternion(-q.x, -q.y, -q.z, q.w);
        }

    }
}
