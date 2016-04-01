using UnityEngine;
using UnityEditor;

namespace IK {
    [CustomEditor(typeof(IKTarget))]
    public class IKTargetEditor : Editor {
        IKTarget t;
        bool moving = false;
        bool rotating = false;

        void OnEnable() {
            t = (IKTarget)target;
        }

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            if(GUILayout.Button("Toggle position editing")) {
                moving = !moving;
                if (moving) rotating = false;                
            }
            if (GUILayout.Button("Toggle normal editing")) {
                rotating = !rotating;
                if (rotating) moving = false;
            }
        }

        void OnSceneGUI() {
            if (t.ContactPoints != null) {
                foreach (IKContactPoint p in t.ContactPoints) {
                    Handles.color = Color.cyan;
                    Handles.DrawLine(p.Location + t.transform.position, p.Normal.normalized * 0.1f + t.transform.position + p.Location);

                    if (moving) {
                        Vector3 newLoc = Handles.DoPositionHandle(p.Location + t.transform.position, Quaternion.LookRotation(p.Normal, Vector3.up)) - t.transform.position;
                        if (newLoc != p.Location) {
                            Undo.RegisterCompleteObjectUndo(t, "moved contact point");
                            p.Location = newLoc;
                        }
                    }
                    if (rotating) {
                        Vector3 newLoc = (Handles.DoPositionHandle(p.Normal.normalized * 0.1f + t.transform.position + p.Location, Quaternion.LookRotation(Vector3.up)) - t.transform.position - p.Location) / 0.1f;
                        if (newLoc != p.Normal) {
                            Undo.RegisterCompleteObjectUndo(t, "moved contact normal");
                            p.Normal = newLoc.normalized;
                        }
                    }
                    Handles.DrawSolidDisc(p.Location + t.transform.position, p.Normal, 0.01f);
                    Handles.Label(p.Location + t.transform.position + p.Normal.normalized * 0.1f, p.Finger.ToString());
                }
            }
        }
    }
}