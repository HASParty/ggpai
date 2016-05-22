using UnityEngine;
using System.Collections;
using Boardgame.Agent;
using UnityEngine.UI;
using Boardgame.Configuration;

namespace Boardgame.UI {
    public class UIAgentStateViewer : MonoBehaviour {

        private Mood mood;
        private PersonalityModule pm;
        public RectTransform ArousalBG;
        public RectTransform ArousalKnob;
        public RectTransform ValenceBG;
        public RectTransform ValenceKnob;
        public RectTransform RawArousalBG;
        public RectTransform RawArousalKnob;
        public RectTransform RawValenceBG;
        public RectTransform RawValenceKnob;

        public Color Low, Neutral, High;

        public Image Extraversion, Openness, Neuroticism, Agreeableness, Conscientiousness;


        // Use this for initialization
        void Start() {
            mood = FindObjectOfType<Mood>();
            pm = FindObjectOfType<PersonalityModule>();
            Set(Extraversion, pm.GetExtraversion());
            Set(Openness, pm.GetOpenness());
            Set(Neuroticism, pm.GetNeuroticism());
            Set(Agreeableness, pm.GetAgreeableness());
            Set(Conscientiousness, pm.GetConscientiousness());
        }

        // Update is called once per frame
        void Update() {
            Set(ArousalBG, ArousalKnob, mood.GetArousal());
            Set(ValenceBG, ValenceKnob, mood.GetValence());
            Set(RawArousalBG, RawArousalKnob, mood.GetArousalRaw());
            Set(RawValenceBG, RawValenceKnob, mood.GetValenceRaw());

        }

        public void Set(Image img, PersonalityModule.PersonalityValue val) {
            if (img == null) return;
            Color col = Neutral;
            switch (val) {
                case PersonalityModule.PersonalityValue.high:
                    col = High;
                    break;
                case PersonalityModule.PersonalityValue.low:
                    col = Low;
                    break;
            }
            img.color = col;
        }

        public void Set(RectTransform bg, RectTransform knob, float val) {
            if (bg == null || knob == null) return;
            float frac = (val - Config.Low) / Config.High;
            float x = bg.rect.width * frac;
            knob.anchoredPosition = new Vector2(x, knob.anchoredPosition.y);
        }
    }
}