using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Boardgame {
	[CustomEditor(typeof(Grid))]
	public class GridEditor : Editor {

		Grid board;
		SerializedProperty scriptable;

		void OnEnable()
		{
			board = (Grid)target;
			scriptable = serializedObject.FindProperty ("Source");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update ();
			EditorGUILayout.PropertyField (scriptable, new GUIContent ("Source"), true);
			serializedObject.ApplyModifiedProperties ();
		}

		void OnSceneGUI()
		{
			if (board.Source.grid != null) {
				foreach (Cell cell in board.Source.grid) {
					Vector3[] loc = new Vector3[4];
					float offsetX = board.transform.position.x - board.transform.localScale.x/2;
					float offsetY = board.transform.position.y + board.transform.localScale.y/2;
					float offsetZ = board.transform.position.z - board.transform.localScale.x/2;
					loc [0] = new Vector3 (cell.x + offsetX, offsetY, cell.y + offsetZ);
					loc [1] = new Vector3 (cell.x + cell.w + offsetX, offsetY, cell.y + offsetZ);
					loc [2] = new Vector3 (cell.x + cell.w + offsetX, offsetY, cell.y + cell.h + offsetZ);
					loc [3] = new Vector3 (cell.x + offsetX, offsetY, cell.y + cell.h + offsetZ);

					Handles.DrawSolidRectangleWithOutline (loc, Color.clear, Color.red);
					Handles.Label(loc[0] + (loc[2] - loc[0])/2, cell.id);
					Handles.DrawWireDisc(loc[0] + (loc[2] - loc[0])/2, Vector3.up, 0.01f);
				}
			}
		}
	

	}

}