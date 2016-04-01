using UnityEngine;
using System.Collections;
using UnityEditor;

namespace IK {
    [CustomEditor(typeof(IKSegment))]
    public class IKSegmentEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            IKSegment me = target as IKSegment;
        }
    }
}
