using Boardgame.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Behaviour {

    public class BMLScheduleDebugger : MonoBehaviour {

        public Color Face;
        public Color Gaze;
        public Color Gesture;
        public Color Head;
        public Color Locomotion;
        public Color Pointing;
        public Color Posture;
        public Color Speech;
        public GameObject DotPrefab;
        public RectTransform ContentBox;
        public Text HoverText;
        public float TimeScaler = 10f;


        // Use this for initialization
        void Start() {

        }

        // Update is called once per frame
        void Update() {

        }

        public void Spawn(BMLChunk chunk) {
            //Debug.Log ("spawninating");
            float x = Time.time * TimeScaler;
            if (x > ContentBox.sizeDelta.x) {
                ContentBox.sizeDelta = new Vector2(x + 20f * TimeScaler, ContentBox.sizeDelta.y);
            }
            Color color;
            Vector2 position = new Vector2(x, Random.Range(0, ContentBox.rect.height - DotPrefab.GetComponent<RectTransform>().rect.height));
            string text = chunk.Character.identikit.actorName + " at " + Time.time + "  (" + chunk.ID + ") ";
            switch (chunk.Type) {
                case BMLChunkType.Face:
                    var fchunk = chunk as Face;
                    text += fchunk.ToString();
                    color = Face;
                    break;
                case BMLChunkType.FaceEmotion:
                    var fechunk = chunk as FaceEmotion;
                    text += fechunk.ToString();
                    color = Face;
                    break;
                case BMLChunkType.Gaze:
                    color = Gaze;
                    var gchunk = chunk as Gaze;
                    text += gchunk.ToString();
                    break;
                case BMLChunkType.GazeShift:
                    var gschunk = chunk as GazeShift;
                    text += gschunk.ToString();
                    color = Gaze;
                    break;
                case BMLChunkType.Gesture:
                    var gechunk = chunk as Gesture;
                    text += gechunk.ToString();
                    color = Gesture;
                    break;
                case BMLChunkType.Head:
                    var hchunk = chunk as Head;
                    text += hchunk.ToString();
                    color = Head;
                    break;
                case BMLChunkType.Locomotion:
                    var lchunk = chunk as Locomotion;
                    text += lchunk.ToString();
                    color = Locomotion;
                    break;
                case BMLChunkType.Posture:
                    var pchunk = chunk as Posture;
                    text += pchunk.ToString() + " ";
                    foreach (var pose in pchunk.Poses) {
                        text += pose.ToString() + " ";
                    }
                    color = Posture;
                    break;
                case BMLChunkType.Speech:
                    color = Speech;
                    var schunk = chunk as Speech;
                    text += schunk.ToString();
                    break;
                case BMLChunkType.Pointing:
                    color = Pointing;
                    Pointing pointchunk = chunk as Pointing;
                    text += pointchunk.ToString();
                    break;
                case BMLChunkType.Placing:
                    color = Pointing;
                    Place placeChunk = chunk as Place;
                    text += placeChunk.ToString();
                    break;
                case BMLChunkType.Grasping:
                    color = Pointing;
                    Place graspChunk = chunk as Place;
                    text += graspChunk.ToString();
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }

            GameObject dot = Instantiate(DotPrefab);
            dot.transform.SetParent(ContentBox.transform, false);
            Image img = dot.GetComponent<Image>();
            RectTransform rt = dot.GetComponent<RectTransform>();
            UIHoverInfo hi = dot.AddComponent<UIHoverInfo>();
            img.CrossFadeColor(color, 0f, true, false);
            rt.anchoredPosition = position;
            hi.Text = text;
            hi.TextField = HoverText;

        }
    }

}
