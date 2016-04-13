using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Boardgame {
    /// <summary>
    /// UI manager. Images etc. set in inspector.
    /// </summary>
    public class UIManager : Singleton<UIManager> {

        public Canvas canvas;
        [SerializeField]
        private Image mouseCursor;
        [SerializeField]
        private Image highlightImage;
        [SerializeField]
        private Vector3 highlightRotate;
        [SerializeField]
        private Vector3 highlightOffset;
        [SerializeField]
        private Image selectImage;
        [SerializeField]
        private Vector3 selectRotate;
        [SerializeField]
        private Vector3 selectOffset;
        [SerializeField]
        private Image legalCellImage;
        [SerializeField]
        private Vector3 legalRotate;
        [SerializeField]
        private Vector3 legalOffset;
        [SerializeField]
        private Text stateText;

        private GameObject highlightObject;
        private GameObject selectObject;
        private List<GameObject> legalCellObjects = new List<GameObject>();

        void Awake() {
            if (canvas == null) {
                canvas = gameObject.GetComponent<Canvas>();
            }
            if (canvas == null) {
                Debug.LogWarning("UIManager: Found no canvas. UI will not render properly.");
            }
        }

        public void ShowHighlightEffect(Vector3 worldPosition) {
            RectTransform rt;

            HideHighlightEffect();

            highlightObject = Instantiate(highlightImage.gameObject);
            highlightObject.transform.SetParent(canvas.transform);

            rt = highlightObject.GetComponent<RectTransform>();
            rt.localScale = highlightImage.gameObject.GetComponent<RectTransform>().localScale;
            rt.position = worldPosition + highlightOffset;
            rt.Rotate(highlightRotate);
        }

        public void SetState(string state) {
            stateText.text = state;
        }

        public void ShowSelectEffect(Vector3 worldPosition)
        {
            RectTransform rt;

            HideSelectEffect();

            selectObject = Instantiate(selectImage.gameObject);
            selectObject.transform.SetParent(canvas.transform);

            rt = selectObject.GetComponent<RectTransform>();
            rt.localScale = selectImage.gameObject.GetComponent<RectTransform>().localScale;
            rt.position = worldPosition + selectOffset;
            rt.Rotate(selectRotate);
        }

        public void HideHighlightEffect() {
            if (highlightObject != null) {
                Destroy(highlightObject);
            }
        }

        public void HideSelectEffect()
        {
            if (selectObject != null)
            {
                Destroy(selectObject);
            }
        }

        public void ShowLegalCells(List<Vector3> cells)
        {
            RectTransform rt;
            if (legalCellObjects.Count > 0)
            {
                HideLegalCells();
            }
            foreach (Vector3 cellPos in cells)
            {
                var legalCell = Instantiate(legalCellImage.gameObject);
                legalCell.transform.SetParent(canvas.transform);
                rt = legalCell.GetComponent<RectTransform>();
                rt.localScale = legalCellImage.gameObject.GetComponent<RectTransform>().localScale;
                rt.position = cellPos + legalOffset;
                rt.Rotate(legalRotate);
                rt.SetAsFirstSibling();
                legalCellObjects.Add(legalCell);
            }

        }

        public void HideLegalCells()
        {
            foreach (GameObject cell in legalCellObjects)
            {
                Destroy(cell);
            }

            legalCellObjects.Clear();
        }
    }
}
