using Boardgame.GDL;
using System.Collections.Generic;
using UnityEngine;

namespace Boardgame.Networking {
    public class GameData {


        public GameData(string data) {
            var r = BoardgameManager.Instance.reader;

            if (r.IsStart(data)) {
                IsStart = true;
            }
            else if (r.IsDone(data))
            {
                State = Terminal.DONE;
            }
            else {                
                MovesMade = r.GetMove(data);
            }

            GameState = r.GetBoardState(data);
            LegalMoves = r.GetLegalMoves(data);
            State = r.GetTerminal(data);
            IsBusy = r.IsBusy(data);

            if (LegalMoves.Count == 0) IsHumanPlayerTurn = false;
        }

        public Player Control { get; protected set; }

        public readonly bool IsStart = false;
        public readonly Terminal State;
        public readonly bool IsHumanPlayerTurn = true;
        public readonly bool IsBusy;

        public readonly List<Move> LegalMoves;
        public readonly List<Move> MovesMade;
        public readonly State GameState;
    }
}
