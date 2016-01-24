using UnityEngine;
using System.Collections;

namespace Boardgame {
public class BoardgameManager : Singleton<BoardgameManager> {

		private Grid grid;
		//Some board game state info, needs to be generic to work for multiple games

		public void SetBoard(Grid grid) {
			this.grid = grid;
		}

		public string[] GetLegalMoves(string cellID) {
			//TODO: implement get legal moves
			//presumably will have to interface with GGP plugin or
			//a plugin which allows it to directly communicate
			//with the GDL stuff in order to maintain game state and
			//check for legal moves
			return null;
		}

		public bool IsLegalMove(string cellFromID, string cellToID) {
			//TODO: implement is legal move check
			return true;
		}
	}
}
