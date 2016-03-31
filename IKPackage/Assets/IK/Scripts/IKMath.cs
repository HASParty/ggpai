using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IK {
    class IKMath {
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

    }
}
