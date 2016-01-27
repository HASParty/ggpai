using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Boardgame {
	[CustomEditor(typeof(GridScriptable))]
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