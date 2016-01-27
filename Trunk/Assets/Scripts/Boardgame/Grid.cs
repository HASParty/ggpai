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
            BoardgameManager.Instance.SetBoard(this);
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

        public Cell[] GetAllCells()
        {
            Cell[] all = new Cell[cells.Count];
            cells.Values.CopyTo(all, 0);
            return all;
        }

        public Piece RemovePiece(string cellID)
        {
            if (cells.ContainsKey(cellID))
            {
                Cell cell = cells[cellID];
                Piece piece = cell.piece;
                cell.piece = null;
                return piece;
                //Must then place piece elsewhere in order to move it, or destroy it.
            }
            Debug.LogWarning("Grid: Illegal cell " + cellID + " or cell empty.");
            return null;
        }

        private Vector3 GetLocalCellPosition(string cellID)
        {
            Cell cell = cells[cellID];
            MeshFilter meshfilter = gameObject.GetComponent<MeshFilter>();
            return new Vector3((cell.x + cell.w / 2) - meshfilter.sharedMesh.bounds.extents.x,
                0.5f, (cell.y + cell.h / 2) - meshfilter.sharedMesh.bounds.extents.z);
        }

        public Vector3 GetCellPosition(string cellID)
        {
            Cell cell = cells[cellID];
            MeshFilter meshfilter = gameObject.GetComponent<MeshFilter>();
            return transform.position + transform.localToWorldMatrix.MultiplyVector(
                new Vector3((cell.x + cell.w / 2) - meshfilter.sharedMesh.bounds.extents.x,
                0.5f, (cell.y + cell.h / 2) - meshfilter.sharedMesh.bounds.extents.z));
        }

        public bool PlacePiece(Piece piece, string cellID, bool first = false)
        {            
            if (cells.ContainsKey(cellID) && (cells[cellID].piece == null || first))
            {
                piece.transform.SetParent(transform);
				piece.Cell = cells[cellID];
                piece.transform.localPosition = GetLocalCellPosition(cellID);
                return true;
            }
            Debug.LogWarning("Grid: Illegal cell " + cellID + " or cell already occupied.");
            return false;
        }
    }
}
