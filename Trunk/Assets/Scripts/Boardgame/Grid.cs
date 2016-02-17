using UnityEngine;
using System.Collections.Generic;
using Boardgame.Script;
using System;

namespace Boardgame {
public class Grid : MonoBehaviour {
		[Tooltip("Game board configuration to load")]
		public GridScriptable Source;
		private Dictionary<string, Cell> cells;
        private Dictionary<string, PhysicalCell> pcells;

        private float boardHalfWidth;
		private float boardHalfHeight;
		private float scaleX;
		private float scaleZ;

        public void LoadScriptable(GridScriptable physicalBoardDescription)
        {
            Source = physicalBoardDescription;
            cells = new Dictionary<string, Cell>();
            pcells = new Dictionary<string, PhysicalCell>();
            foreach(Cell cell in Source.grid)
            {
                PhysicalCell pcell = ScriptHelper<PhysicalCell>.spawn(transform);
                pcell.gameObject.name = cell.id;
                pcell.id = cell.id;
                pcell.SetColliderRadius(cell.w / 2);
                cells.Add(cell.id, cell);
                pcells.Add(cell.id, pcell);
                pcell.transform.localPosition = GetLocalCellPosition(cell.id);
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

        public PhysicalCell[] GetAllPhysicalCells()
        {
            PhysicalCell[] all = new PhysicalCell[pcells.Count];
            pcells.Values.CopyTo(all, 0);
            return all;
        }

        public GameObject RemovePiece(string cellID)
        {
            if (pcells.ContainsKey(cellID))
            {
                return pcells[cellID].RemovePiece();
            }
            else
            {
                Debug.LogWarning("Grid: Illegal cell " + cellID + " or cell empty.");
                return null;
            }
        }

        private Vector3 GetLocalCellPosition(string cellID)
        {
            Cell cell = cells[cellID];
            MeshFilter meshfilter = gameObject.GetComponent<MeshFilter>();
            return new Vector3((cell.x + cell.w / 2) - meshfilter.sharedMesh.bounds.extents.x,
                0.5f, (cell.y + cell.h / 2) - meshfilter.sharedMesh.bounds.extents.z);
        }

        public Vector3 GetWhiteHandPosition()
        {
            Cell cell = Source.WhiteHand;
            MeshFilter meshfilter = gameObject.GetComponent<MeshFilter>();
            return new Vector3((cell.x + cell.w / 2) - meshfilter.sharedMesh.bounds.extents.x,
                0.5f, (cell.y + cell.h / 2) - meshfilter.sharedMesh.bounds.extents.z);
        }

        public Vector3 GetBlackHandPosition()
        {
            Cell cell = Source.BlackHand;
            MeshFilter meshfilter = gameObject.GetComponent<MeshFilter>();
            return new Vector3((cell.x + cell.w / 2) - meshfilter.sharedMesh.bounds.extents.x,
                0.5f, (cell.y + cell.h / 2) - meshfilter.sharedMesh.bounds.extents.z);
        }

        public Vector3 GetCellPosition(string cellID)
        {
            return pcells[cellID].transform.position;
        }

        public bool PlacePiece(GameObject go, string cellID, bool isInstantiated = true)
        {            
            if (cells.ContainsKey(cellID) && !pcells[cellID].HasPiece())
            {
                pcells[cellID].Place(go, isInstantiated);
                return true;
            }
            Debug.LogWarning("Grid: Illegal cell " + cellID + " or cell already occupied.");
            return false;
        }

    }
}
