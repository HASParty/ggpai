using UnityEngine;
using System.Collections;
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
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
