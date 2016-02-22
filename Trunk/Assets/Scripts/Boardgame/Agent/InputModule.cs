using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

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

        }

        public void MakeMove(List<KeyValuePair<string, string>> moves, Player player) {
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
