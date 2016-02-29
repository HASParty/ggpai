using UnityEngine;
using UnityEngine.UI;

namespace Boardgame.UI {

    public class UILookAt : MonoBehaviour {
        public Transform Target;

        void Awake() {
            if (Target == null) Target = Camera.main.transform;
        }

        void Update() {
            GetComponent<RectTransform>().LookAt(Target);
            GetComponent<RectTransform>().Rotate(new Vector3(0, 180, 0));
        }

    }
}
