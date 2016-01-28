using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Boardgame {
	[CustomEditor(typeof(Script.GridScriptable))]
	public class GridScriptableEditor : Editor {
		
		void OnEnable()
		{
		}
		
		public override void OnInspectorGUI() {
			serializedObject.Update ();
			DrawDefaultInspector ();
			serializedObject.ApplyModifiedProperties ();
		}
	}
	
}