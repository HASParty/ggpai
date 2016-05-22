using UnityEngine;
using System.Collections.Generic;
using System;
using Boardgame.Networking;

namespace Boardgame {
    /// <summary>
    /// Manages player interactions with the game.
    /// </summary>
    public class PlayerInteractionManager : Singleton<PlayerInteractionManager> {

        private PhysicalCell selectedPiece;
        private PhysicalCell currentPiece;
        [SerializeField]
        private Player player = Player.Second;

        private bool updated = false;
        private bool waitingForUpdate = false;
        private bool gameOver = false;

        void Start() {
            selectedPiece = null;
            currentPiece = null;
            ConnectionMonitor.Instance.OnGameUpdate.AddListener(OnGameUpdate);
        }


        /// <summary>
        /// Called when the game state is updated.
        /// </summary>
        /// <param name="data">the data from the GGP AI</param>
        public void OnGameUpdate(GameData data) {
            updated = true;
            waitingForUpdate = false;
            //if the state is terminal
            if (data.State != GDL.Terminal.FALSE) {
                //hide all UI effects
                UIManager.Instance.HideHighlightEffect();
                UIManager.Instance.HideLegalCells();
                UIManager.Instance.HideSelectEffect();
                selectedPiece = null;
                currentPiece = null;
                gameOver = true;
            }
        }

        public void LegalUpdate() {
            updated = true;
        }

        void Update() {
            if (ConnectionMonitor.Instance.other != player) player = ConnectionMonitor.Instance.other;
            //sub optimal
            ShowLegalCells();
        }

        /// <summary>
        /// highlights piece if possible
        /// </summary>
        /// <param name="piece">which piece</param>
        public void PieceHighlight(PhysicalCell piece) {
            if (gameOver) return;
            if (BoardgameManager.Instance.CanSelectPiece(piece, player)) {
                if (piece == currentPiece || piece == selectedPiece)
                    return;
                currentPiece = piece;
                UIManager.Instance.ShowHighlightEffect(currentPiece.transform.position);
            }
        }

        /// <summary>
        /// highlights cell if possible
        /// </summary>
        /// <param name="cell">which cell</param>
        public void CellHighlight(PhysicalCell cell) {
            if (gameOver) return;
            if (BoardgameManager.Instance.CanSelectCell(cell, selectedPiece)) {
                if (cell == currentPiece) return;
                currentPiece = cell;
                UIManager.Instance.ShowHighlightEffect(currentPiece.transform.position);
            }
        }

        /// <summary>
        /// Select the cell if possible
        /// </summary>
        /// <param name="cell">which cell</param>
        public void CellSelect(PhysicalCell cell) {
            if (gameOver) return;
            if (BoardgameManager.Instance.CanSelectCell(cell, selectedPiece)) {
                if (selectedPiece == null || cell.HasPiece()) return;
                if (BoardgameManager.Instance.MakeMove(selectedPiece.id, cell.id, player)) {
                    UIManager.Instance.HideSelectEffect();
                    UIManager.Instance.HideHighlightEffect();
                    selectedPiece = null;
                    waitingForUpdate = true;
                }
            }
        }

        /// <summary>
        /// Focus leaves the cell
        /// </summary>
        /// <param name="piece">the cell in question</param>
        public void PieceLeave(PhysicalCell piece) {
            if (gameOver) return;
            if (piece == currentPiece) {
                UIManager.Instance.HideHighlightEffect();
                currentPiece = null;
            }
        }

        /// <summary>
        /// Select piece if possible
        /// </summary>
        /// <param name="piece">which piece</param>
        public void PieceSelect(PhysicalCell piece) {
            if (gameOver) return;
            if (BoardgameManager.Instance.CanSelectPiece(piece, player)) {
                if (piece == selectedPiece) return;
                UIManager.Instance.HideHighlightEffect();
                if (BoardgameManager.Instance.IsRemoveablePiece(piece)) {
                    BoardgameManager.Instance.MakeMove(piece.id, null, player);
                    waitingForUpdate = true;
                    UIManager.Instance.HideHighlightEffect();
                } else {
                    if (piece == selectedPiece)
                        return;
                    if (piece == currentPiece) {
                        PieceLeave(piece);
                    }
                    selectedPiece = piece;
                    UIManager.Instance.ShowSelectEffect(selectedPiece.transform.position);
                }
            }
        }

        string lastPiece;
        /// <summary>
        /// Display all the available cells for selection
        /// </summary>
        private void ShowLegalCells() {
            if (gameOver) return;
            if (waitingForUpdate) {
                UIManager.Instance.HideLegalCells();
                return;
            }
            var cid = " ";
            if (currentPiece != null && currentPiece.HasPiece()) {
                cid = currentPiece.id;
            } else if (selectedPiece != null) {
                cid = selectedPiece.id;
            }
            if (lastPiece == cid && !updated) return;
            lastPiece = cid;
            updated = false;
            UIManager.Instance.HideLegalCells();
            var legalMoves = BoardgameManager.Instance.GetLegalMoves(cid, player);
            List<Vector3> cellPos = new List<Vector3>();
            foreach (string id in legalMoves) {
                cellPos.Add(BoardgameManager.Instance.GetCellPosition(id));
            }
            UIManager.Instance.ShowLegalCells(cellPos);
        }
    }
}
