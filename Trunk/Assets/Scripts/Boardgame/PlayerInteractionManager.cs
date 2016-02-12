using UnityEngine;
using System.Collections.Generic;
using System;

namespace Boardgame {
	public class PlayerInteractionManager : Singleton<PlayerInteractionManager> {

    	private PhysicalCell selectedPiece;
		private PhysicalCell currentPiece;
        [SerializeField]
        private Player player = Player.White;

		// Use this for initialization
		void Start () {
			selectedPiece = null;
			currentPiece = null;
		}

		public void PieceHighlight(PhysicalCell piece) {
			//TODO: check if piece belongs to player and can actually be highlighted
			if (piece == currentPiece || piece == selectedPiece)
				return;
			currentPiece = piece;
			UIManager.Instance.ShowHighlightEffect (currentPiece.transform.position);
		}

        public void CellHighlight(PhysicalCell cell)
        {
            if (cell == currentPiece || selectedPiece == null) return;
            currentPiece = cell;
            UIManager.Instance.ShowHighlightEffect(currentPiece.transform.position);
        }

        public void CellSelect(PhysicalCell cell)
        {
            if (selectedPiece == null || cell.HasPiece()) return;
            if (BoardgameManager.Instance.MakeMove(selectedPiece.id, cell.id, player))
            {
                UIManager.Instance.HideSelectEffect();
                selectedPiece = null;
                UIManager.Instance.HideLegalCells();
            }
        }

        public void PieceLeave(PhysicalCell piece) {
			if (piece == currentPiece) {
				UIManager.Instance.HideHighlightEffect();
				currentPiece = null;
			}
		}

		public void PieceSelect(PhysicalCell piece) {
            if (piece == selectedPiece) return;
            UIManager.Instance.HideLegalCells();
            if (BoardgameManager.Instance.BelongsToPlayer(piece.id, player))
            {
                if (piece == selectedPiece)
                    return;
                if (piece == currentPiece)
                {
                    PieceLeave(piece);
                }
                selectedPiece = piece;
            }
            UIManager.Instance.ShowSelectEffect(selectedPiece.transform.position);
            ShowLegalCells();
        }

        private void ShowLegalCells()
        {
            var legalMoves = BoardgameManager.Instance.GetLegalMoves(selectedPiece.id, player);
            List<Vector3> cellPos = new List<Vector3>();
            foreach (string id in legalMoves)
            {
                cellPos.Add(BoardgameManager.Instance.GetCellPosition(id));
            }
            UIManager.Instance.ShowLegalCells(cellPos);
        }
	}
}
