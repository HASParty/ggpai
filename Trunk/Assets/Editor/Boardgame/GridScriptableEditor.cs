using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Boardgame {
	[CustomEditor(typeof(GridScriptable))]
	public class GridScriptableEditor : Editor {
		SerializedProperty grid;
		
		void OnEnable()
		{
			grid = serializedObject.FindProperty("grid");
		}
		
		public override void OnInspectorGUI() {
			serializedObject.Update ();
			DrawDefaultInspector ();
			serializedObject.ApplyModifiedProperties ();
		}
	}
	
}