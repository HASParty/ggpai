using UnityEngine;
using System.Collections.Generic;
using Boardgame.Script;
using UnityEngine.Events;
using Boardgame.GDL;
using System;
using Boardgame.Networking;

namespace Boardgame {

    public enum Player { Black, White };

    public class BoardgameManager : Singleton<BoardgameManager> {

        [SerializeField]
        private BoardgameScriptable gameScriptable;

        public Transform BoardSpawnLocation;
        public Transform PileSpawnTransform;

        [System.Serializable]
        public class MoveEvent : UnityEvent<List<Move>, Player> { }

        public MoveEvent OnMakeMove;


        private Player turn;
        private Grid grid;
        private Dictionary<string, GameObject> piecePrefabs;
        //Some board game state info, needs to be generic to work for multiple games

        public GameReader reader;
        public GameWriter writer;


        void Awake() {
            if (gameScriptable != null) {
                reader = Activator.CreateInstance(Type.GetType("Boardgame.GDL."+gameScriptable.ID+"Reader")) as GameReader;
                writer = Activator.CreateInstance(Type.GetType("Boardgame.GDL."+gameScriptable.ID+"Writer")) as GameWriter;

                piecePrefabs = new Dictionary<string, GameObject>();

                grid = Instantiate(gameScriptable.PhysicalBoardPrefab).AddComponent<Grid>();
                grid.LoadScriptable(gameScriptable.PhysicalBoardDescription);
                grid.transform.SetParent(BoardSpawnLocation);
                grid.transform.localPosition = Vector3.zero;
                foreach (var pieces in gameScriptable.PhysicalPieces) {
                    piecePrefabs.Add(pieces.Type, pieces.Prefab);
                }

                ConnectionMonitor.Instance.OnGameUpdate.AddListener(CheckGame);

            } else {
                Debug.LogError("No gamescriptable set!");
            }
        }

        public void CheckGame(GameData data) {
            if (data.IsStart) GameStart(data.GameState);
            SetLegalMoves(data.LegalMoves);
            turn = data.GameState.Control;
            if (data.LegalMoves.Count > 0) Debug.Log(Tools.Stringify<Move>.List(data.LegalMoves));
            else Debug.Log("No legal moves currently.");
        }

        public void GameStart(State state) {
            foreach(GDL.Cell cell in state.Cells) {
                if (piecePrefabs.ContainsKey(cell.Type)) {
                    if (cell.Count > 0) {
                        grid.SetPile(cell.ID);
                        for (int i = 0; i < cell.Count; i++) {
                            GameObject p = Instantiate(piecePrefabs[cell.Type]);
                            p.AddComponent<Rigidbody>();
                            grid.PlacePiece(p, cell.ID);
                            p.transform.SetParent(PileSpawnTransform);
                        }                     
                    } else {
                        grid.PlacePiece(piecePrefabs[cell.Type], cell.ID, false);
                    }
                }
            }
        }

        List<string> placeableIDs = new List<string>();
        List<string> removeableIDs = new List<string>();
        Dictionary<string, List<string>> moveableIDS = new Dictionary<string, List<string>>();
        public void SetLegalMoves(List<Move> moves) {
            placeableIDs.Clear();
            removeableIDs.Clear();
            moveableIDS.Clear();
            foreach(Move m in moves) {
                switch(m.Type) {
                    case MoveType.MOVE:
                        if(moveableIDS.ContainsKey(m.From)) {
                            moveableIDS[m.From].Add(m.To);
                        } else {
                            List<string> vals = new List<string>();
                            vals.Add(m.To);
                            moveableIDS.Add(m.From, vals);
                        }
                        break;
                    case MoveType.REMOVE:
                        removeableIDs.Add(m.From);
                        break;
                    case MoveType.PLACE:
                        placeableIDs.Add(m.To);
                        break;
                }
            }
        }

        public Vector3 GetCellPosition(string id) {
            return grid.GetCellPosition(id);
        }

        public void MakeMove(List<Move> moves) {
            foreach(var move in moves) {
                Debug.Log(move);
                GameObject piece;
                switch (move.Type) {
                    case MoveType.PLACE:
                        string heap = turn == Player.Black ? gameScriptable.BlackPile : gameScriptable.WhitePile;
                        piece = grid.RemovePiece(heap);
                        grid.PlacePiece(piece, move.To);
                        break;
                    case MoveType.REMOVE:
                        piece = grid.RemovePiece(move.From);
                        Destroy(piece);
                        break;
                    case MoveType.MOVE:
                        piece = grid.RemovePiece(move.From);
                        grid.PlacePiece(piece, move.To);
                        break;
                }
            }
            //notify listeners the move has been made
            OnMakeMove.Invoke(moves, turn);
        }

        public bool CanSelectCell(string cellID) {
            string pile = turn == Player.Black ? gameScriptable.BlackPile : gameScriptable.WhitePile;
            if (placeableIDs.Count > 0 && cellID == pile) return true;
            if (removeableIDs.Contains(cellID)) return true;
            if (moveableIDS.ContainsKey(cellID)) return true;
            return false;
        }

        public List<string> GetLegalMoves(string cellID) {
            if(cellID == gameScriptable.WhitePile || cellID == gameScriptable.BlackPile) {
                return placeableIDs;
            }
            return moveableIDS[cellID];
        }


        //player makes move
        public bool MakeMove(string cellFromID, string cellToID) {
            //TODO: characters physically move pieces
            //TODO: update game state
            Move move;
            string pile = turn == Player.Black ? gameScriptable.BlackPile : gameScriptable.WhitePile;
            if(cellFromID == pile) {
                move = new Move(MoveType.PLACE, cellToID);
            } else { 
                move = new Move(cellFromID, cellToID);
            }

            List<Move> moves = new List<Move>();
            moves.Add(move);
            OnMakeMove.Invoke(moves, turn);
            return true;
        }
    }
}
