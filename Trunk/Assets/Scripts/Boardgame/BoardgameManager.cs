using UnityEngine;
using System.Collections.Generic;
using Boardgame.Script;

namespace Boardgame {

public enum Player { Black, White };

public class BoardgameManager : Singleton<BoardgameManager> {

        [SerializeField]
        private BoardgameScriptable gameScriptable;

        public Transform BoardSpawnLocation;

		private Grid grid;
        private Dictionary<string, GameObject> whitePiecePrefabs;
        private Dictionary<string, GameObject> blackPiecePrefabs;
        //Some board game state info, needs to be generic to work for multiple games


        void Awake()
        {
            if (gameScriptable != null) {
                whitePiecePrefabs = new Dictionary<string, GameObject>();
                blackPiecePrefabs = new Dictionary<string, GameObject>();
                grid = Instantiate(gameScriptable.PhysicalBoardPrefab).AddComponent<Grid>();
                grid.LoadScriptable(gameScriptable.PhysicalBoardDescription);
                grid.transform.SetParent(BoardSpawnLocation);
                grid.transform.localPosition = Vector3.zero;
                foreach(var pieces in gameScriptable.PhysicalPieces)
                {
                    if(pieces.Player == Player.White)
                    {
                        whitePiecePrefabs.Add(pieces.Type, pieces.Prefab);
                    } else
                    {
                        blackPiecePrefabs.Add(pieces.Type, pieces.Prefab);
                    }
                }
                foreach(var white in gameScriptable.InitialWhitePieces)
                {
                    if (!white.notOnBoard)
                    {
                        grid.PlacePiece(whitePiecePrefabs[white.pieceType], white.cellID, false);
                    }

                }
                foreach (var black in gameScriptable.InitialBlackPieces)
                {
                    if (!black.notOnBoard)
                    {
                        grid.PlacePiece(blackPiecePrefabs[black.pieceType], black.cellID, false);
                    }
                }
            }
        }

        public Vector3 GetCellPosition(string id)
        {
            return grid.GetCellPosition(id);
        }

		public List<string> GetLegalMoves(string cellID, Player player) {
            //TODO: implement get legal moves
            //presumably will have to interface with GGP plugin or
            //a plugin which allows it to directly communicate
            //with the GDL stuff in order to maintain game state and
            //check for legal moves

            //for now just returns empty cells
            List<string> moves = new List<string>();
            foreach(PhysicalCell cell in grid.GetAllPhysicalCells())
            {
                if(!cell.HasPiece())
                {
                    moves.Add(cell.id);
                }
            }
            return moves;
		}

        public bool BelongsToPlayer(string cellFromID, Player player)
        {
            //TODO: check if piece at cell belongs to player
            return true;
        }

		public bool IsLegalMove(string cellFromID, string cellToID, Player player) {
            //TODO: actually implement is legal move
            return GetLegalMoves(cellFromID, player).Contains(cellToID);
		}

        public bool MakeMove(string cellFromID, string cellToID, Player player)
        {
            //TODO: characters physically move pieces
            //TODO: update game state
            if(IsLegalMove(cellFromID, cellToID, player))
            {
                GameObject piece = grid.RemovePiece(cellFromID);
                grid.PlacePiece(piece, cellToID);
                return true;
            }
            return false;
        }
	}
}
