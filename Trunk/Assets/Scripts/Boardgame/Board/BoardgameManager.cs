using UnityEngine;
using System.Collections.Generic;
using Boardgame.Script;
using UnityEngine.Events;
using Boardgame.GDL;
using System;
using Boardgame.Networking;
using System.Collections;

namespace Boardgame {

    public enum Player { First, Second };
    /// <summary>
    /// Manages the boardgame. Spawns the board, pieces, and so on. Moving pieces,
    /// validity checks for player interactions, etc.
    /// </summary>
    public class BoardgameManager : Singleton<BoardgameManager> {

        [SerializeField]
        private BoardgameScriptable gameScriptable;

        public Transform BoardSpawnLocation;
        public Transform PileSpawnTransform;

        [SerializeField]
        private AudioClip piecePlaceSound;

        [SerializeField]
        private AudioSource turnChangeSource;

        [System.Serializable]
        public class MoveEvent : UnityEvent<List<Move>, Player> { }

        [System.Serializable]
        public class GameEnd : UnityEvent { }

        public GameEnd OnGameEnd;
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

        public GameObject GetCellObject(string id) {
            return grid.GetPhysicalCell(id).gameObject;
        }

        Player last;
        void Update() {
            if (turn != last) {
                turnChangeSource.Play();
                last = turn;
            }
        }

        bool moveInProgress = false;
        IEnumerator WaitForMove(UnityAction action) {
            while (moveInProgress) yield return new WaitForSeconds(0.2f);
            //Debug.Log("I have waited a million years");
            action();
        }

        public void CheckGame(GameData data) {
            moveInProgress = true;
            if (!data.IsDone && data.State == Terminal.FALSE) {
                if (data.IsStart) GameStart(data.GameState);
                else {
                    SetState(data.GameState);
                    if (data.LegalMoves.Count == 0 && data.MovesMade.Count == 0) SyncState();
                }
                
                if (data.IsHumanPlayerTurn)
                {
                    if (last == Player.Second) moveInProgress = false;
                    StartCoroutine(WaitForMove(() => {
                        SetLegalMoves(data.LegalMoves);
                        turn = Player.Second;
                        UIManager.Instance.SetState("Player's turn");
                    }));                   
                }
                else
                {
                    if (last == Player.Second) moveInProgress = false;
                    SetLegalMoves(data.LegalMoves);
                    turn = Player.First;
                    UIManager.Instance.SetState("AI's turn");
                }
                
            } else {
                //Debug.Log(data.State);
                SetLegalMoves(new List<Move>());
                UIManager.Instance.SetState(data.State.ToString());
            }
        }

        State state;
        void SetState(State state) {
            this.state = state;
        }

        public bool IsBusy() {
            return moveInProgress;
        }

        public void SyncState() {
            if (state.Cells == null) return;
            //Debug.Log("synchronising");
            //TODO: make not disgusting
            //UPDATE: I made it more disgusting, in a way
            foreach (var cell in grid.GetAllCells()) {
                if (grid.IsPile(cell.id)) continue;
                grid.Clear(cell.id);
            }
            GameStart(state, true);      
        }

        public void GameStart(State state, bool skipPiles = false) {
            grid.SetPile(gameScriptable.TrashPile);
            foreach(GDL.Cell cell in state.Cells) {
                if (piecePrefabs.ContainsKey(cell.Type)) {
                    if (grid.IsPile(cell.ID) && skipPiles) continue;
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
        Dictionary<string, List<string>> moveableIDs = new Dictionary<string, List<string>>();
        Dictionary<string, List<string>> captureableIDs = new Dictionary<string, List<string>>();
        public void SetLegalMoves(List<Move> moves) {
            placeableIDs.Clear();
            removeableIDs.Clear();
            moveableIDs.Clear();
            captureableIDs.Clear();
            foreach(Move m in moves) {
                switch(m.Type) {
                    case MoveType.CAPTURE:
                        if (captureableIDs.ContainsKey(m.From)) {
                            captureableIDs[m.From].Add(m.To);
                        } else {
                            List<string> vals = new List<string>();
                            vals.Add(m.To);
                            captureableIDs.Add(m.From, vals);
                        }
                        break;
                    case MoveType.MOVE:
                        if(moveableIDs.ContainsKey(m.From)) {
                            moveableIDs[m.From].Add(m.To);
                        } else {
                            List<string> vals = new List<string>();
                            vals.Add(m.To);
                            moveableIDs.Add(m.From, vals);
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
            PlayerInteractionManager.Instance.LegalUpdate();
        }

        public Vector3 GetCellPosition(string id) {
            return grid.GetCellPosition(id);
        }

        public void MakeMove(List<Move> moves, Player player) {
            GameObject piece = null;
            foreach(var move in moves) {
                Debug.Log(move);
                switch (move.Type) {
                    case MoveType.PLACE:
                        string heap = player == Player.First ? gameScriptable.FirstPile : gameScriptable.SecondPile;
                        piece = grid.RemovePiece(heap);
                        grid.PlacePiece(piece, move.To);
                        grid.PlaySoundAt(move.To, piecePlaceSound);
                        break;
                    case MoveType.REMOVE:
                        piece = grid.RemovePiece(move.From);
                        grid.PlacePiece(piece, gameScriptable.TrashPile);
                        grid.PlaySoundAt(move.From, piecePlaceSound);
                        break;
                    case MoveType.CAPTURE:
                    case MoveType.MOVE:
                        piece = grid.RemovePiece(move.From);
                        grid.PlacePiece(piece, move.To);
                        grid.PlaySoundAt(move.To, piecePlaceSound);
                        break;
                }
            }
            moveInProgress = false;
            //notify listeners the move has been made
            OnMakeMove.Invoke(moves, player);
        }

        public void GetMoveFromTo(Move move, Player player, out PhysicalCell from, out PhysicalCell to) {
            switch (move.Type) {
                case MoveType.PLACE:
                    string heap = player == Player.First ? gameScriptable.FirstPile : gameScriptable.SecondPile;
                    from = grid.GetPhysicalCell(heap);
                    to = grid.GetPhysicalCell(move.To);
                    break;
                case MoveType.REMOVE:
                    from = grid.GetPhysicalCell(move.From);
                    to = grid.GetPhysicalCell(gameScriptable.TrashPile);
                    break;
                case MoveType.CAPTURE:
                case MoveType.MOVE:
                    from = grid.GetPhysicalCell(move.From);
                    to = grid.GetPhysicalCell(move.To);
                    break;
                default:
                    from = null;
                    to = null;
                    break;
            }
        } 

        public void MoveMade(List<Move> moves, Player player) {
            moveInProgress = false;
            OnMakeMove.Invoke(moves, player);
        }

        public bool CanSelectCell(PhysicalCell cell, PhysicalCell selected) {
            if (selected == null) return false;
            if (grid.IsPile(selected.id) && placeableIDs.Contains(cell.id)) return true;
            if (moveableIDs.ContainsKey(selected.id) && moveableIDs[selected.id].Contains(cell.id)) return true;
            if (captureableIDs.ContainsKey(selected.id) && captureableIDs[selected.id].Contains(cell.id)) return true;
            return false;
        } 

        public bool CanSelectPiece(PhysicalCell cell, Player player) {
            string pile = player == Player.First ? gameScriptable.FirstPile : gameScriptable.SecondPile;
            if (placeableIDs.Count > 0 && cell.id == pile) return true;
            if (removeableIDs.Contains(cell.id)) return true;
            if (moveableIDs.ContainsKey(cell.id) || captureableIDs.ContainsKey(cell.id)) return true;
            return false;
        }

        public List<string> GetLegalMoves(string cellID, Player player) {
            string pile = player == Player.First ? gameScriptable.FirstPile : gameScriptable.SecondPile;
            if (cellID == pile) {
                return placeableIDs;
            } else if (cellID.Trim() == "") {
                List<string> legals = new List<string>();
                legals.AddRange(removeableIDs);
                legals.AddRange(moveableIDs.Keys);
                legals.AddRange(captureableIDs.Keys);
                if (placeableIDs.Count > 0) {
                    legals.Add(pile);
                }
                return legals;
            } else if (moveableIDs.ContainsKey(cellID)) {
                List<string> legals = new List<string>();
                legals.AddRange(moveableIDs[cellID]);
                legals.AddRange(moveableIDs.Keys);
                return legals;
            } else if (captureableIDs.ContainsKey(cellID)) {
                List<string> legals = new List<string>();
                legals.AddRange(captureableIDs[cellID]);
                legals.AddRange(captureableIDs.Keys);
                return legals;
            }
            return new List<string>();
        }

        public void MakeNoise(string cellID) {
            grid.PlaySoundAt(cellID, piecePlaceSound);
        }

        public bool IsRemoveablePiece(PhysicalCell piece) {
            return removeableIDs.Contains(piece.id);
        }

        //player makes move
        public bool MakeMove(string cellFromID, string cellToID, Player player) {
            Move move;
            string pile = player == Player.First ? gameScriptable.FirstPile : gameScriptable.SecondPile;
            if (cellFromID == pile) {
                move = new Move(MoveType.PLACE, cellToID);
            } else if (cellToID == null) {
                move = new Move(MoveType.REMOVE, cellFromID);
            } else if(captureableIDs.ContainsKey(cellFromID) && captureableIDs[cellFromID].Contains(cellToID)) {  
                move = new Move(MoveType.CAPTURE, cellFromID, cellToID);
            } else {
                move = new Move(cellFromID, cellToID);
            }

            List<Move> moves = new List<Move>();
            moves.Add(move);
            MakeMove(moves, player);
            return true;
        }
    }
}
