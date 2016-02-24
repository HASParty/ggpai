using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using Boardgame.Networking;

namespace Boardgame.Agent
{

    [RequireComponent(typeof(PersonalityModule), typeof(BrainModule))]
    public class InputModule : MonoBehaviour
    {
        private PersonalityModule pm;
        private BrainModule bm;

        void Start()
        {
            pm = GetComponent<PersonalityModule>();
            bm = GetComponent<BrainModule>();
            BoardgameManager.Instance.OnMakeMove.AddListener(MakeMove);
            //if we want to be able to support multiple agents, they should refer
            //to their instance of a connection monitor, and connection monitor should not
            //be a singleton
            ConnectionMonitor.Instance.OnFeedUpdate.AddListener(CheckStatus);
            ConnectionMonitor.Instance.OnGameUpdate.AddListener(CheckGame);

        }

        public void CheckGame(GameData data) {
            if(data.IsStart && data.LegalMoves.Count == 0) {
                bm.player = data.GameState.Control;
                Debug.Log(bm.player);
            }
            if (!data.IsStart && bm.player == data.Control && data.MovesMade.Count > 0) {
                var move = data.MovesMade;
                BoardgameManager.Instance.MakeMove(move, bm.player);
            }
        }

        public void CheckStatus(FeedData data) {
            
        }

        public void MakeMove(List<GDL.Move> moves, Player player) {
            Debug.Log("InputModule acknowledging a move has been made");
            if(player == bm.player) {
                Debug.Log("It was my own move, I don't really need to do anything.");
            } else {
                Debug.Log("This information should be passed through a GameWriter and on to the AI.");

            }
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}
