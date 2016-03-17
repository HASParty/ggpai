using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using Boardgame.Networking;
using Boardgame.Configuration;

namespace Boardgame.Agent {

    [RequireComponent(typeof(PersonalityModule), typeof(BrainModule))]
    public class InputModule : MonoBehaviour {
        private PersonalityModule pm;
        private BrainModule bm;

        private bool isMyTurn = false;

        void Start() {
            pm = GetComponent<PersonalityModule>();
            bm = GetComponent<BrainModule>();
            BoardgameManager.Instance.OnMakeMove.AddListener(OnMoveMade);
            //if we want to be able to support multiple agents, they should refer
            //to their instance of a connection monitor, and connection monitor should not
            //be a singleton
            ConnectionMonitor.Instance.OnFeedUpdate.AddListener(CheckStatus);
            ConnectionMonitor.Instance.OnGameUpdate.AddListener(CheckGame);

        }

        public void CheckGame(GameData data) {
            if (data.IsStart && data.LegalMoves.Count == 0) {
                bm.player = Player.First;
            } else if(data.IsStart) {
                bm.player = Player.Second;
            }
            if (!data.IsStart && bm.player == data.Control && data.MovesMade.Count > 0) {
                var move = data.MovesMade;
                bm.ExecuteMove(move);
            }

            isMyTurn = !data.IsHumanPlayerTurn;
        }

        public void CheckStatus(FeedData data) {
            bm.EvaluateConfidence(data.FirstUCT, data.SecondUCT);
            bm.ConsiderMove(data.Best);
            if(data.MaxSimulation > Config.SimulationCutoff && isMyTurn) {
				ConnectionMonitor.Instance.ModifyRequestTime(0.33f);
            }
        }

        public void OnMoveMade(List<GDL.Move> moves, Player player) {
            Debug.Log("InputModule acknowledging a move has been made");
            if (player == bm.player) {
               
            } else {
                Debug.Log("This information should be passed through a GameWriter and on to the AI.");

            }
        }
    }
}
