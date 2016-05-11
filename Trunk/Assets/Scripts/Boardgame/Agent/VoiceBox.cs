using UnityEngine;
using Boardgame.Configuration;
namespace Boardgame.Agent {
    [RequireComponent(typeof(AudioSource))]
    public class VoiceBox : MonoBehaviour {
        [SerializeField]
        AudioClip[] positivelyExcited;
        [SerializeField]
        AudioClip[] negativelyExcited;
        [SerializeField]
        AudioClip[] neutral;
        [SerializeField]
        AudioClip[] positivelyCalm;
        [SerializeField]
        AudioClip[] negativelyCalm;

        AudioSource src;

        void Awake() {
            src = GetComponent<AudioSource>();
        }

        public void Vocalise(float arousal, float valence) {
            if (arousal >= Config.Neutral) {
                if (valence >= Config.Neutral) {
                    Play(positivelyExcited);
                } else {
                    Play(negativelyExcited);
                }
            } else {
                if (valence >= Config.Neutral) {
                    Play(positivelyCalm);
                } else {
                    Play(negativelyCalm);
                }
            }
        }

        void Play(AudioClip[] select) {
            if (select == null || select.Length == 0) return;
            int ind = Random.Range(0, select.Length);
            src.PlayOneShot(select[ind]);
        }
    }
}
