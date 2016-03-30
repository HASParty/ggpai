using UnityEngine;
using System.Collections;
using UnityEditor;

namespace IK {
    [CustomEditor(typeof(IKSegment))]
    public class IKSegmentEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            IKSegment me = target as IKSegment;
           /* EditorGUILayout.Vector3Field("Original pitch/yaw/roll", IKCCD.GetPitchYawRollDeg(me.GetBaseRotation()));
            EditorGUILayout.Vector3Field("Current pitch/yaw/roll", IKCCD.GetPitchYawRollDeg(me.transform.rotation));
            EditorGUILayout.Vector3Field("Offset pitch/yaw/roll", IKCCD.GetPitchYawRollDeg(me.transform.rotation)-IKCCD.GetPitchYawRollDeg(me.GetBaseRotation()));*/
        }
    }
}
