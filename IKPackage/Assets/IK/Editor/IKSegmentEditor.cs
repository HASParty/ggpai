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
        SerializedProperty end;

        bool editingAxes = false;


        void OnEnable() {
            seg = (IKSegment)target;

            type = serializedObject.FindProperty("JointType");
            coneRad = serializedObject.FindProperty("ConeRadius");
            damping = serializedObject.FindProperty("DampDegrees");
            easeIn = serializedObject.FindProperty("EaseIn");
            easeOut = serializedObject.FindProperty("EaseOut");
            end = serializedObject.FindProperty("End");
        }

        public override void OnInspectorGUI() {
            EditorGUI.BeginChangeCheck();
            seg.Constrain = EditorGUILayout.BeginToggleGroup("Joint Constraints", seg.Constrain);
            if(GUILayout.Button("Reset parent offset")) {
                seg.ParentOffset = Quaternion.identity;
            }
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
                    if (GUILayout.Button("Toggle test x")) {
                        seg.TestX();
                    }
                    EditorGUILayout.PropertyField(end);
                    break;
                case IKJointType.TwoDOF:
                    EditorGUILayout.BeginHorizontal();
                    seg.Min.x = EditorGUILayout.FloatField(seg.Min.x);
                    seg.Max.x = EditorGUILayout.FloatField(seg.Max.x);
                    EditorGUILayout.EndHorizontal();
                    if (GUILayout.Button("Toggle test x")) {
                        seg.TestX();
                    }
                    EditorGUILayout.BeginHorizontal();
                    seg.Min.y = EditorGUILayout.FloatField(seg.Min.y);
                    seg.Max.y = EditorGUILayout.FloatField(seg.Max.y);
                    EditorGUILayout.EndHorizontal();
                    if (GUILayout.Button("Toggle test y")) {
                        seg.TestY();
                    }
                    EditorGUILayout.PropertyField(end);
                    break;


            }
            EditorGUILayout.EndToggleGroup();
            seg.Twist = EditorGUILayout.BeginToggleGroup("Twisting", seg.Twist);
            if(GUILayout.Button("Toggle test twist")) {
                seg.TestTwist();
            }
            EditorGUILayout.BeginHorizontal();
            seg.Min.z = EditorGUILayout.FloatField(seg.Min.z);
            seg.Max.z = EditorGUILayout.FloatField(seg.Max.z);
            EditorGUILayout.EndHorizontal();
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
                drawHinge(seg.ParentOffset*seg.transform.parent.up, seg.Min.x, seg.Max.x, Color.red);
                if (seg.JointType == IKJointType.TwoDOF) {
                    drawHinge(seg.ParentOffset*seg.transform.parent.forward, seg.Min.y, seg.Max.y, Color.yellow);
                }

                Quaternion newOffset = Handles.RotationHandle(seg.ParentOffset, seg.transform.position);
                if(newOffset != seg.ParentOffset) {
                    Undo.RegisterCompleteObjectUndo(seg, "Rotation offset");
                    seg.ParentOffset = newOffset;
                }
            }

            if (seg.Twist) {
                Vector3 normal = seg.transform.right;
                Quaternion twist = IKMath.GetTwist(normal, seg.transform.rotation);
                Vector3 a = IKMath.GetTwist(normal, seg.GetBaseRotation()) * Vector3.up;
                Vector3 b = twist * Vector3.up;
                float angle = IKMath.AngleOnPlane(a, b, normal);
                Vector3 up = seg.transform.up;
                Handles.color = Color.red;
                Handles.DrawLine(seg.transform.position, (Quaternion.AngleAxis(angle, normal) * up) * 0.1f + seg.transform.position);
                Handles.color = Color.cyan;
                Handles.DrawLine(seg.transform.position, (Quaternion.AngleAxis(seg.Min.z, normal) * up) * 0.1f + seg.transform.position);
                Handles.DrawLine(seg.transform.position, (Quaternion.AngleAxis(seg.Max.z, normal) * up) * 0.1f + seg.transform.position);

                Handles.DrawLine(seg.transform.position, seg.transform.position + normal);

                Handles.DrawWireArc(seg.transform.position, normal, up, seg.Min.z, 0.1f);
                Handles.DrawWireArc(seg.transform.position, normal, up, seg.Max.z, 0.1f);
            }
        }

        void drawHinge(Vector3 axis, float min, float max, Color col) {
            Vector3 x = -axis;
            Handles.color = col;
            Vector3 dir = seg.transform.parent.rotation * seg.originalDir;
            Handles.DrawLine(seg.transform.position - x.normalized * 0.1f, x.normalized * 0.1f + seg.transform.position);
            Handles.color = Color.cyan;
            Handles.DrawWireArc(seg.transform.position, x, dir, min, 0.2f);
            Handles.DrawWireArc(seg.transform.position, x, dir, max, 0.2f);
        }
    }
}
