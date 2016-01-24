using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager> {

	public Canvas canvas;
	[SerializeField]
	private Image highlightImage;
	private GameObject highlightObject;

	void Awake() {
		if (canvas == null) {
			canvas = gameObject.GetComponent<Canvas> ();
		}
		if (canvas == null) {
			Debug.LogWarning ("UIManager: Found no canvas. UI will not render properly.");
		}
	}

	public void ShowHighlightArrow(Vector3 worldPosition) {
		RectTransform rt;
		if (highlightObject == null) {
			highlightObject = Instantiate (highlightImage.gameObject);
			highlightObject.transform.SetParent (canvas.transform);
			rt = highlightObject.GetComponent<RectTransform> ();
			rt.localScale = highlightImage.gameObject.GetComponent<RectTransform> ().localScale;
			rt.position = worldPosition;
		} else {
			rt = highlightObject.GetComponent<RectTransform> ();
			rt.position = worldPosition;
		}
	}

	public void HideHighlightArrow() {
		if (highlightObject != null) {
			GameObject.Destroy (highlightObject);
		}
	}
}
