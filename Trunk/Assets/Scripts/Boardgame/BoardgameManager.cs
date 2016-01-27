using UnityEngine;
using System.Collections.Generic;

namespace Boardgame {

public enum Player { Black, White };

public class BoardgameManager : Singleton<BoardgameManager> {

		private Grid grid;
		//Some board game state info, needs to be generic to work for multiple games

		public void SetBoard(Grid grid) {
			this.grid = grid;
		}

        public Vector3 GetCellPosition(string id)
        {
            return grid.GetCellPosition(id);
        }

		public List<string> GetLegalMoves(string cellID) {
            //TODO: implement get legal moves
            //presumably will have to interface with GGP plugin or
            //a plugin which allows it to directly communicate
            //with the GDL stuff in order to maintain game state and
            //check for legal moves

            //for now just returns empty cells
            List<string> moves = new List<string>();
            foreach(Cell cell in grid.GetAllCells())
            {
                if(cell.piece == null)
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
            if (BelongsToPlayer(cellFromID, player))
            {
                return GetLegalMoves(cellFromID).Contains(cellToID);
            }
            return false;
		}

        public bool MakeMove(string cellFromID, string cellToID, Player player)
        {
            //TODO: characters physically move pieces
            if(IsLegalMove(cellFromID, cellToID, player))
            {
                Piece piece = grid.RemovePiece(cellFromID);
                grid.PlacePiece(piece, cellToID);
                return true;
            }
            return false;
        }
	}
}
