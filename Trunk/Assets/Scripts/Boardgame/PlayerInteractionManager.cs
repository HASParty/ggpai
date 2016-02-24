using UnityEngine;
using System.Collections.Generic;
using System;
using Boardgame.Networking;

namespace Boardgame {
    public class PlayerInteractionManager : Singleton<PlayerInteractionManager> {

        private PhysicalCell selectedPiece;
        private PhysicalCell currentPiece;
        [SerializeField]
        private Player player = Player.Second;

        // Use this for initialization
        void Start() {
            selectedPiece = null;
            currentPiece = null;
        }

        void Update() {
            if (ConnectionMonitor.Instance.other != player) player = ConnectionMonitor.Instance.other;
        }

        public void PieceHighlight(PhysicalCell piece) {
            if (BoardgameManager.Instance.CanSelectPiece(piece)) {
                if (piece == currentPiece || piece == selectedPiece)
                    return;
                currentPiece = piece;
                UIManager.Instance.ShowHighlightEffect(currentPiece.transform.position);
            }
        }

        public void CellHighlight(PhysicalCell cell) {
            if (BoardgameManager.Instance.CanSelectCell(cell, selectedPiece)) {
                if (cell == currentPiece) return;
                currentPiece = cell;
                UIManager.Instance.ShowHighlightEffect(currentPiece.transform.position);
            }
        }

        public void CellSelect(PhysicalCell cell) {
            if (BoardgameManager.Instance.CanSelectCell(cell, selectedPiece)) {
                if (selectedPiece == null || cell.HasPiece()) return;
                if (BoardgameManager.Instance.MakeMove(selectedPiece.id, cell.id)) {
                    UIManager.Instance.HideSelectEffect();
                    UIManager.Instance.HideHighlightEffect();
                    selectedPiece = null;
                    UIManager.Instance.HideLegalCells();
                }
            }
        }

        public void PieceLeave(PhysicalCell piece) {
            if (piece == currentPiece) {
                UIManager.Instance.HideHighlightEffect();
                currentPiece = null;
            }
        }

        public void PieceSelect(PhysicalCell piece) {
            if (BoardgameManager.Instance.CanSelectPiece(piece)) {
                if (piece == selectedPiece) return;
                UIManager.Instance.HideLegalCells();
                UIManager.Instance.HideHighlightEffect();
                if (BoardgameManager.Instance.IsRemoveablePiece(piece)) {
                    BoardgameManager.Instance.MakeMove(piece.id, null);
                    UIManager.Instance.HideHighlightEffect();
                } else {
                    if (piece == selectedPiece)
                    return;
                    if (piece == currentPiece) {
                        PieceLeave(piece);
                    }                
                    selectedPiece = piece;
                    UIManager.Instance.ShowSelectEffect(selectedPiece.transform.position);
                    ShowLegalCells();
                }
            }
        }

        private void ShowLegalCells() {
            var legalMoves = BoardgameManager.Instance.GetLegalMoves(selectedPiece.id);
            List<Vector3> cellPos = new List<Vector3>();
            foreach (string id in legalMoves) {
                cellPos.Add(BoardgameManager.Instance.GetCellPosition(id));
            }
            UIManager.Instance.ShowLegalCells(cellPos);
        }
    }
}
