using UnityEngine;
using UnityEditor;

namespace IK {
    [CustomEditor(typeof(IKTarget))]
    public class IKTargetEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
        }
    }
}