using UnityEngine;
using System.Collections;
using UnityEditor;

namespace Boardgame {
	[CustomEditor(typeof(Grid))]
	public class GridEditor : Editor {

		Grid board;
		SerializedProperty scriptable;
		int selectedCell = 0;
		static bool editingEnabled = false;

		void OnEnable()
		{
			board = (Grid)target;
			scriptable = serializedObject.FindProperty ("Source");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update ();
			EditorGUILayout.PropertyField (scriptable, new GUIContent ("Source"), true);
			if(!editingEnabled && GUILayout.Button ("Enable editing")) {
				editingEnabled = true;
				SceneView.RepaintAll();

			} else if (editingEnabled && GUILayout.Button ("Disable editing")) {
				editingEnabled = false;
				SceneView.RepaintAll();
			}
			serializedObject.ApplyModifiedProperties ();
		}

		void OnSceneGUI()
		{
			if (board.Source.grid != null) {
				MeshFilter meshfilter = board.gameObject.GetComponent<MeshFilter>();
				//if(meshfilter == null) return;
				float scaleX = board.transform.localScale.x;
				float scaleZ = board.transform.localScale.z;
				float offsetX = board.transform.position.x - meshfilter.sharedMesh.bounds.extents.x*scaleX;
				float offsetY = board.transform.position.y + meshfilter.sharedMesh.bounds.extents.y*board.transform.localScale.y;
				float offsetZ = board.transform.position.z - meshfilter.sharedMesh.bounds.extents.z*scaleZ;
				for(int i = 0; i < board.Source.grid.Length; i++) {
					Cell cell = board.Source.grid[i];
					Vector3[] loc = new Vector3[4];


					loc [0] = new Vector3 (cell.x*scaleX + offsetX, offsetY, cell.y*scaleZ + offsetZ);
					loc [1] = new Vector3 ((cell.x + cell.w)*scaleX + offsetX, offsetY, cell.y*scaleZ + offsetZ);
					loc [2] = new Vector3 ((cell.x + cell.w)*scaleX + offsetX, offsetY, (cell.y + cell.h)*scaleZ + offsetZ);
					loc [3] = new Vector3 (cell.x*scaleX + offsetX, offsetY, (cell.y + cell.h)*scaleZ + offsetZ);

					Vector3 center = loc[0] + (loc[2] - loc[0])/2;

					Handles.DrawSolidRectangleWithOutline (loc, Color.clear, Color.red);
					Handles.Label(center, cell.id);
					if(editingEnabled) {
						if(selectedCell == i) {
							Vector3 newPos = Handles.PositionHandle(center, board.transform.rotation);
							if(GUI.changed) {
								board.Source.grid[i].x -= center.x-newPos.x;
								board.Source.grid[i].y -= center.z-newPos.z;
							}
						}
						if(Handles.Button (center, board.transform.rotation, 0.01f, 0.01f, Handles.SphereCap)){
							selectedCell = i;
						}
					}

				}
			}
		}
	

	}

}