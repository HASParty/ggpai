using UnityEngine;
using System.Collections;
namespace Boardgame.Agent
{
    [RequireComponent(typeof(PersonalityModule), typeof(InputModule))]
    public class BrainModule : MonoBehaviour
    {
        //Generate FML here? 
        private PersonalityModule pm;
        private InputModule im;

        public Player player;

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
