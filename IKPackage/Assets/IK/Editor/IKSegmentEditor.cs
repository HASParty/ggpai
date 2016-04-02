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

        string[] axes;

        void OnEnable() {
            seg = (IKSegment)target;
            axes = new string[3];
            axes[0] = "x";
            axes[1] = "y";
            axes[2] = "z";

            type = serializedObject.FindProperty("JointType");
            coneRad = serializedObject.FindProperty("ConeRadius");
            damping = serializedObject.FindProperty("DampDegrees");
            easeIn = serializedObject.FindProperty("EaseIn");
            easeOut = serializedObject.FindProperty("EaseOut");
        }

        public override void OnInspectorGUI() {
            seg.Constrain = EditorGUILayout.BeginToggleGroup("Joint Constraints", seg.Constrain);
           EditorGUILayout.PropertyField(type);
            switch (seg.JointType) {
                case IKJointType.Cone:
                    EditorGUILayout.PropertyField(coneRad);
                    break;
                case IKJointType.OneDOF:
                    float min, max;
                    EditorGUILayout.BeginHorizontal();
                    seg.DOF1 = EditorGUILayout.Popup(seg.DOF1, axes);
                    getMinAndMax(seg.DOF1, out min, out max);
                    min = EditorGUILayout.FloatField(min);
                    max = EditorGUILayout.FloatField(max);
                    setMinAndMax(seg.DOF1, min, max);
                    EditorGUILayout.EndHorizontal();
                    break;
                case IKJointType.TwoDOF:
                    float minb, maxb;
                    EditorGUILayout.BeginHorizontal();
                    seg.DOF1 = EditorGUILayout.Popup(seg.DOF1, axes);
                    if (seg.DOF2 == seg.DOF1) seg.DOF1 = (seg.DOF1 + 1) % 3;
                    getMinAndMax(seg.DOF1, out min, out max);
                    min = EditorGUILayout.FloatField(min);
                    max = EditorGUILayout.FloatField(max);
                    setMinAndMax(seg.DOF1, min, max);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    seg.DOF2 = EditorGUILayout.Popup(seg.DOF2, axes);
                    if (seg.DOF2 == seg.DOF1) seg.DOF2 = (seg.DOF2 + 1) % 3;
                    getMinAndMax(seg.DOF2, out minb, out maxb);
                    minb = EditorGUILayout.FloatField(minb);
                    maxb = EditorGUILayout.FloatField(maxb);
                    setMinAndMax(seg.DOF2, minb, maxb);
                    EditorGUILayout.EndHorizontal();
                    break;


            }

            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.Separator();
            EditorGUILayout.PropertyField(damping);
            EditorGUILayout.PropertyField(easeIn);
            EditorGUILayout.PropertyField(easeOut);




            serializedObject.ApplyModifiedProperties();
        }

        private void getMinAndMax(int selected, out float min, out float max) {
            switch(selected) {
                case 0:
                    min = seg.Min.x;
                    max = seg.Max.x;
                    break;
                case 1:
                    min = seg.Min.y;
                    max = seg.Max.y;
                    break;
                case 2:
                    min = seg.Min.z;
                    max = seg.Max.z;
                    break;
                default:
                    min = 0;
                    max = 0;
                    break;
            }
        }

        private void setMinAndMax(int selected, float min, float max) {
            switch (selected) {
                case 0:
                    seg.Min.x = min;
                    seg.Max.x = max;
                    break;
                case 1:
                    seg.Min.y = min;
                    seg.Max.y = max;
                    break;
                case 2:
                    seg.Min.z = min;
                    seg.Max.z = max;
                    break;
            }
        }
    }
}
