using UnityEngine;
using System.Collections;
using Boardgame.Agent;
using UnityEngine.UI;
using Boardgame.Configuration;

namespace Boardgame.UI {
    public class UIMoodViewer : MonoBehaviour {

        private PersonalityModule pm;
        public RectTransform ArousalBG;
        public RectTransform ArousalKnob;
        public RectTransform ValenceBG;
        public RectTransform ValenceKnob;


        // Use this for initialization
        void Start() {
            pm = FindObjectOfType<PersonalityModule>();
        }

        // Update is called once per frame
        void Update() {
            Set(ArousalBG, ArousalKnob, pm.GetArousal());
            Set(ValenceBG, ValenceKnob, pm.GetValence());
        }

        public void Set(RectTransform bg, RectTransform knob, float val) {
            float frac = (val - Config.Low) / Config.High;
            float x = bg.rect.width * frac;
            knob.anchoredPosition = new Vector2(x, knob.anchoredPosition.y);
        }
    }
}