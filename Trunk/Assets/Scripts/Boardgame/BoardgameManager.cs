using UnityEngine;
using System.Collections.Generic;
using Boardgame.Script;
using UnityEngine.Events;
using Boardgame.GDL;

namespace Boardgame {

    public enum Player { Black, White };

    public class BoardgameManager : Singleton<BoardgameManager> {

        [SerializeField]
        private BoardgameScriptable gameScriptable;

        public Transform BoardSpawnLocation;
        public Transform PileSpawnTransform;

        [System.Serializable]
        public class MoveEvent : UnityEvent<List<KeyValuePair<string, string>>, Player> { }

        public MoveEvent OnMakeMove;


        private Player turn;
        private Grid grid;
        private Dictionary<string, GameObject> whitePiecePrefabs;
        private Dictionary<string, GameObject> blackPiecePrefabs;
        //Some board game state info, needs to be generic to work for multiple games

        private GameReader reader;
        private GameWriter writer;


        void Awake() {
            if (gameScriptable != null) {
                whitePiecePrefabs = new Dictionary<string, GameObject>();
                blackPiecePrefabs = new Dictionary<string, GameObject>();
                grid = Instantiate(gameScriptable.PhysicalBoardPrefab).AddComponent<Grid>();
                grid.LoadScriptable(gameScriptable.PhysicalBoardDescription);
                if (gameScriptable.pieceInHandCount > 0) {
                    grid.SetPile(gameScriptable.whitePile);
                    grid.SetPile(gameScriptable.blackPile);
                }
                grid.transform.SetParent(BoardSpawnLocation);
                grid.transform.localPosition = Vector3.zero;
                foreach (var pieces in gameScriptable.PhysicalPieces) {
                    if (pieces.Player == Player.White) {
                        whitePiecePrefabs.Add(pieces.Type, pieces.Prefab);
                    } else {
                        blackPiecePrefabs.Add(pieces.Type, pieces.Prefab);
                    }
                }
                foreach (var white in gameScriptable.InitialWhitesOnBoard) {
                    grid.PlacePiece(whitePiecePrefabs[white.pieceType], white.cellID, false);
                }

                for (int i = 0; i < gameScriptable.pieceInHandCount; i++) {
                    GameObject w = Instantiate(whitePiecePrefabs[gameScriptable.pieceTypeInHand]);
                    GameObject b = Instantiate(blackPiecePrefabs[gameScriptable.pieceTypeInHand]);
                    w.AddComponent<Rigidbody>();
                    b.AddComponent<Rigidbody>();
                    grid.PlacePiece(w, gameScriptable.whitePile, pile: true);
                    grid.PlacePiece(b, gameScriptable.blackPile, pile: true);
                    w.transform.SetParent(PileSpawnTransform);
                    b.transform.SetParent(PileSpawnTransform);
                }
                foreach (var black in gameScriptable.InitialBlackPieces) {
                    grid.PlacePiece(blackPiecePrefabs[black.pieceType], black.cellID, false);
                }
            } else {
                Debug.LogError("No gamescriptable set!");
            }
        }

        public Vector3 GetCellPosition(string id) {
            return grid.GetCellPosition(id);
        }

        public void MakeMove(List<KeyValuePair<string,string>> moves, Player player) {
            foreach(var move in moves) {
                var fromOrType = move.Key;
                var to = move.Value;
                Debug.Log(fromOrType + to);
            }
            //notify listeners the move has been made
            OnMakeMove.Invoke(moves, player);
        }

        public List<string> GetLegalMoves(string cellID, Player player) {
            //TODO: implement get legal moves
            //presumably will have to interface with GGP plugin or
            //a plugin which allows it to directly communicate
            //with the GDL stuff in order to maintain game state and
            //check for legal moves
        
            //for now just returns empty cells
            List<string> moves = new List<string>();
            foreach (PhysicalCell cell in grid.GetAllPhysicalCells()) {
                if (!cell.HasPiece()) {
                    moves.Add(cell.id);
                }
            }
            return moves;
        }

        public bool BelongsToPlayer(string cellFromID, Player player) {
            //TODO: check if piece at cell belongs to player
            return true;
        }

        public bool IsLegalMove(string cellFromID, string cellToID, Player player) {
            //TODO: actually implement is legal move
            return GetLegalMoves(cellFromID, player).Contains(cellToID);
        }

        public bool MakeMove(string cellFromID, string cellToID, Player player) {
            //TODO: characters physically move pieces
            //TODO: update game state
            
            if (IsLegalMove(cellFromID, cellToID, player)) {
                GameObject piece = grid.RemovePiece(cellFromID);
                grid.PlacePiece(piece, cellToID);
                return true;
            }
            return false;
        }
    }
}
