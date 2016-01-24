using UnityEngine;
using System.Collections.Generic;

namespace Boardgame {
public class Grid : MonoBehaviour {
		[Tooltip("Game board configuration to load")]
		public GridScriptable Source;
		private Dictionary<string, Cell> cells;

		private float boardHalfWidth;
		private float boardHalfHeight;
		private float scaleX;
		private float scaleZ;

		public void Awake() {
			PopulateBoard ();
			BoardgameManager.Instance.SetBoard (this);
			MeshFilter meshfilter = gameObject.GetComponent<MeshFilter>();
		}

		public void PopulateBoard() {
			if (Source == null) {
				Debug.LogError ("Grid: Game board uninitialised. Please make sure to have set a board.");
				return;
			}
			cells = new Dictionary<string, Cell>();
            foreach (Cell cell in Source.grid)
            {
                cells.Add(cell.id, cell);
                if (cell.piece != null)
                {
                    Piece piece = Instantiate(cell.piece);
                    PlacePiece(piece, cell.id, true);
                }
			}
		}

		public Cell GetCell(string cellID) {
			return cells [cellID];
		}

        public bool PlacePiece(Piece piece, string cellID, bool first = false)
        {            
            if (cells.ContainsKey(cellID) && (cells[cellID].piece == null || first))
            {
				Cell cell = cells[cellID];
				MeshFilter meshfilter = gameObject.GetComponent<MeshFilter>();
                piece.transform.SetParent(transform);
				piece.Cell = cell;
				piece.transform.localPosition = new Vector3((cell.x + cell.w/2) - meshfilter.sharedMesh.bounds.extents.x, 0.5f, (cell.y + cell.h/2) - meshfilter.sharedMesh.bounds.extents.z);
                return true;
            }
            Debug.LogWarning("Grid: Illegal cell " + cellID + " or cell already occupied.");
            return false;
        }
    }
}
