using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

namespace IK {
    public class IKWrapper {
        //figure out a way to determine this via preprocessing
        private const string dll = "IK64";

        [DllImport(dll)]
        private static extern int GetRandom();

        public static int GetRand() {
            return GetRandom();
        }
    }
}
