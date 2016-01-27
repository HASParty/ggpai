using UnityEngine;
using System.Collections;
namespace Boardgame.Agent
{

    [RequireComponent(typeof(PersonalityModule), typeof(BrainModule), typeof(GameModule))]
    public class InputModule : MonoBehaviour
    {
        private PersonalityModule pm;
        private BrainModule bm;
        private GameModule gm;

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
