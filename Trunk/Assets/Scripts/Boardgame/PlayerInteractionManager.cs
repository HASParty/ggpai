using UnityEngine;
using System.Collections.Generic;

namespace Boardgame {
	public class PlayerInteractionManager : Singleton<PlayerInteractionManager> {

    	private Piece selectedPiece;
		private Piece currentPiece;
        [SerializeField]
        private Player player = Player.Black;

		// Use this for initialization
		void Start () {
			selectedPiece = null;
			currentPiece = null;
		}

        void Update ()
        {
        }


		public void PieceHighlight(Piece piece) {
			//TODO: check if piece belongs to player and can actually be highlighted
			if (piece == currentPiece || piece == selectedPiece)
				return;
			currentPiece = piece;
			UIManager.Instance.ShowHighlightEffect (currentPiece.GetHighlightPosition ());
		}

		public void PieceLeave(Piece piece) {
			if (piece == currentPiece) {
				UIManager.Instance.HideHighlightEffect();
				currentPiece = null;
			}
		}

		public void PieceSelect(Piece piece) {
            UIManager.Instance.HideLegalCells();
            if (BoardgameManager.Instance.BelongsToPlayer(piece.Cell.id, player))
            {
                if (piece == selectedPiece)
                    return;
                if (piece == currentPiece)
                {
                    PieceLeave(piece);
                }
                selectedPiece = piece;
            }
            UIManager.Instance.ShowSelectEffect(selectedPiece.GetSelectPosition());
            var legalMoves = BoardgameManager.Instance.GetLegalMoves(selectedPiece.Cell.id);
            List<Vector3> cellPos = new List<Vector3>();
            foreach(string id in legalMoves)
            {
                cellPos.Add(BoardgameManager.Instance.GetCellPosition(id));
            }
            UIManager.Instance.ShowLegalCells(cellPos);

        }
	}
}
