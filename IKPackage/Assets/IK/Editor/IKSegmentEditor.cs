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
        SerializedProperty x, y, z;

        string[] axes;

        void OnEnable() {
            seg = (IKSegment)target;
            axes = new string[3];
            axes[0] = "x";
            axes[1] = "y";
            axes[2] = "z";

            /*if (seg.transform.childCount > 0) {
                seg.originalDir = (seg.transform.GetChild(0).transform.position - seg.transform.position).normalized;
            }*/

            type = serializedObject.FindProperty("JointType");
            coneRad = serializedObject.FindProperty("ConeRadius");
            damping = serializedObject.FindProperty("DampDegrees");
            easeIn = serializedObject.FindProperty("EaseIn");
            easeOut = serializedObject.FindProperty("EaseOut");
            x = serializedObject.FindProperty("x");
            y = serializedObject.FindProperty("y");
            //z = serializedObject.FindProperty("z");
        }

        public override void OnInspectorGUI() {
            seg.Constrain = EditorGUILayout.BeginToggleGroup("Joint Constraints", seg.Constrain);
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
                    break;


            }

            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(damping);
            EditorGUILayout.PropertyField(easeIn);
            EditorGUILayout.PropertyField(easeOut);




            serializedObject.ApplyModifiedProperties();
        }

        void OnSceneGUI() {
            if (seg.JointType == IKJointType.OneDOF || seg.JointType == IKJointType.TwoDOF) {
                Vector3 x = -(seg.transform.rotation * seg.x);
                Handles.DrawLine(seg.transform.position - x.normalized * 0.1f, x.normalized * 0.1f + seg.transform.position);
                Handles.color = Color.cyan;
                Handles.DrawWireArc(seg.transform.position, x, seg.originalDir, seg.Min.x, 0.2f);
                Handles.DrawWireArc(seg.transform.position, x, seg.originalDir, seg.Max.x, 0.2f);
                Vector3 newLoc = (Handles.DoPositionHandle(x.normalized * 0.1f + seg.transform.position, Quaternion.LookRotation(seg.transform.forward)) - seg.transform.position) / 0.1f;
                if (newLoc != x) {
                    Undo.RegisterCompleteObjectUndo(seg, "moved x axis");
                    seg.x = (Quaternion.Inverse(seg.transform.rotation)*newLoc).normalized;
                }

                if(seg.JointType == IKJointType.TwoDOF) {
                    Handles.DrawLine(seg.transform.position - seg.y.normalized * 0.1f, seg.y.normalized * 0.1f + seg.transform.position);
                    newLoc = (Handles.DoPositionHandle(seg.y.normalized * 0.1f + seg.transform.position, Quaternion.LookRotation(seg.transform.forward)) - seg.transform.position) / 0.1f;
                    if (newLoc != seg.y) {
                        Undo.RegisterCompleteObjectUndo(seg, "moved y axis");
                        seg.y = newLoc.normalized;
                    }
                }
            }
        }
    }
}
