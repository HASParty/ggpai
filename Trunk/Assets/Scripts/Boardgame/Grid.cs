using UnityEngine;
using System.Collections.Generic;

namespace Boardgame {
public class Grid : MonoBehaviour {
		[Tooltip("Game board configuration to load")]
		public GridScriptable Source;
		private Dictionary<string, Cell> cells;

		public void Awake() {
			PopulateBoard ();
		}

		public void PopulateBoard() {
			if (Source == null) {
				Debug.LogError ("Grid: Game board uninitialised. Please make sure to have set a board.");
				return;
			}
			cells = new Dictionary<string, Cell>();
			foreach(Cell cell in Source.grid) {
				cells.Add(cell.id, cell);
				if(cell.piece != null) {
					Piece piece = Instantiate(cell.piece);
					piece.transform.SetParent(transform);
					piece.transform.localPosition = new Vector3(-transform.localScale.x/2 + cell.x + cell.w/2, transform.localPosition.y, -transform.localScale.z/2 + cell.y + cell.h/2);
				}
			}
		}
	}
}
