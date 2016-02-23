using Boardgame.GDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Boardgame.Networking {
    public class GameData {


        public GameData(string data) {
            var r = BoardgameManager.Instance.reader;

            if (r.IsStart(data)) {
                IsStart = true;
            } else {                
                MovesMade = r.GetMove(data);
                IsStart = false;
            }

            GameState = r.GetBoardState(data);
            LegalMoves = r.GetLegalMoves(data);
        }

        public Player Control { get; protected set; }

        public bool IsStart {
            get;
            protected set;
        }
        public List<Move> LegalMoves {
            get;
            protected set;
        }
        public List<Move> MovesMade {
            get;
            protected set;
        }
        public State GameState {
            get;
            protected set;
        }
    }
}
