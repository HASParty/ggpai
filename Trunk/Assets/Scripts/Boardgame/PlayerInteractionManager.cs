using UnityEngine;
using System.Collections;

namespace Boardgame {
	public class PlayerInteractionManager : Singleton<PlayerInteractionManager> {


		private Piece selectedPiece;
		private Piece currentPiece;

		// Use this for initialization
		void Start () {
			selectedPiece = null;
			currentPiece = null;
		}


		public void PieceHighlight(Piece piece) {
			//TODO: check if piece belongs to player and can actually be highlighted
			if (piece == currentPiece || piece == selectedPiece)
				return;
			currentPiece = piece;
			UIManager.Instance.ShowHighlightArrow (currentPiece.GetHighlightPosition ());
		}

		public void PieceLeave(Piece piece) {
			if (piece == currentPiece) {
				UIManager.Instance.HideHighlightArrow();
				currentPiece = null;
			}
		}

		public void PieceSelect(Piece piece) {
			//TODO: check if this a piece we can actually pick
			if (piece == selectedPiece)
				return;
			if (piece == currentPiece) {
				PieceLeave(piece);
			}
			selectedPiece = piece;
			//TODO: show piece being selected

			//TODO: show legal cells to which piece can be moved

		}
	}
}
