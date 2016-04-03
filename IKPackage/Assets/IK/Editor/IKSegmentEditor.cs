using UnityEngine;
using System.Collections;
using UnityEditor;

namespace IK {
    [CustomEditor(typeof(IKSegment))]
    public class IKSegmentEditor : Editor {
        IKSegment seg;
        SerializedProperty type;
        SerializedProperty coneRad;
        SerializedProperty damping;
        SerializedProperty easeIn;
        SerializedProperty easeOut;
        SerializedProperty end, hingeAbout;
        SerializedProperty x, y, z;

        bool editingAxes = false;

        void OnEnable() {
            seg = (IKSegment)target;

            type = serializedObject.FindProperty("JointType");
            coneRad = serializedObject.FindProperty("ConeRadius");
            damping = serializedObject.FindProperty("DampDegrees");
            easeIn = serializedObject.FindProperty("EaseIn");
            easeOut = serializedObject.FindProperty("EaseOut");
            end = serializedObject.FindProperty("End");
            hingeAbout = serializedObject.FindProperty("HingeAbout");
            x = serializedObject.FindProperty("x");
            y = serializedObject.FindProperty("y");
            //z = serializedObject.FindProperty("z");
        }

        public override void OnInspectorGUI() {
            EditorGUI.BeginChangeCheck();
            seg.Constrain = EditorGUILayout.BeginToggleGroup("Joint Constraints", seg.Constrain);
            editingAxes = EditorGUILayout.ToggleLeft("Axis editing", editingAxes);
            if (GUILayout.Button("Recalculate origin")) {
                seg.Init();
            }
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(type);
            switch (seg.JointType) {
                case IKJointType.Cone:
                    EditorGUILayout.PropertyField(coneRad);
                    break;
                case IKJointType.OneDOF:
                    EditorGUILayout.BeginHorizontal();
                    seg.Min.x = EditorGUILayout.FloatField(seg.Min.x);
                    seg.Max.x = EditorGUILayout.FloatField(seg.Max.x);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.PropertyField(x);
                    EditorGUILayout.PropertyField(end);
                    break;
                case IKJointType.TwoDOF:
                    EditorGUILayout.BeginHorizontal();
                    seg.Min.x = EditorGUILayout.FloatField(seg.Min.x);
                    seg.Max.x = EditorGUILayout.FloatField(seg.Max.x);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.PropertyField(x);
                    EditorGUILayout.BeginHorizontal();
                    seg.Min.y = EditorGUILayout.FloatField(seg.Min.y);
                    seg.Max.y = EditorGUILayout.FloatField(seg.Max.y);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.PropertyField(y);
                    EditorGUILayout.PropertyField(end);
                    break;


            }
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(damping);
            EditorGUILayout.PropertyField(easeIn);
            EditorGUILayout.PropertyField(easeOut);

            if (EditorGUI.EndChangeCheck()) {
                SceneView.RepaintAll();
            }


            serializedObject.ApplyModifiedProperties();
        }

        void OnSceneGUI() {
            if (seg.Constrain && (seg.JointType == IKJointType.OneDOF || seg.JointType == IKJointType.TwoDOF)) {
                drawHinge(ref seg.x, seg.Min.x, seg.Max.x);
                if (seg.JointType == IKJointType.TwoDOF) {
                    drawHinge(ref seg.y, seg.Min.y, seg.Max.y);
                }
            }
        }

        void drawHinge(ref Vector3 axis, float min, float max) {
            Vector3 x = (seg.transform.parent.rotation * axis);
            Vector3 dir = seg.transform.parent.rotation * seg.originalDir;
            Handles.DrawLine(seg.transform.position - x.normalized * 0.1f, x.normalized * 0.1f + seg.transform.position);
            Handles.color = Color.cyan;
            Handles.DrawWireArc(seg.transform.position, x, dir, min, 0.2f);
            Handles.DrawWireArc(seg.transform.position, x, dir, max, 0.2f);
            if (editingAxes) {
                Vector3 newLoc = (Handles.DoPositionHandle(x.normalized * 0.1f + seg.transform.position, Quaternion.LookRotation(seg.transform.forward)) - seg.transform.position) / 0.1f;
                if (newLoc != x) {
                    Undo.RegisterCompleteObjectUndo(seg, "moved axis of "+seg.name);
                    axis = (Quaternion.Inverse(seg.transform.rotation) * newLoc).normalized;
                }
            }
        }
    }
}
