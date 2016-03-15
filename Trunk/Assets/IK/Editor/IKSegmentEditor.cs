using UnityEngine;
using System.Collections;
using UnityEditor;

namespace IK {
    [CustomEditor(typeof(IKSegment))]
    public class IKSegmentEditor : Editor {
        IKSegment segment;

        void OnEnable() {
            segment = target as IKSegment;
            segment.Init();
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
        }

        public void OnSceneGUI() {
            UnityEditor.Tools.current = Tool.None;
            EditorGUI.BeginChangeCheck();
            Quaternion rot = Handles.RotationHandle(segment.transform.rotation, segment.JointBase+segment.OriginalPosition);
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(target, "Rotated Segment");
                segment.Rotate(rot.eulerAngles);
            }
        }
    }
}
