using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Boardgame.UI {
	
	public class UIHoverInfo : MonoBehaviour, IPointerEnterHandler {
		[Tooltip("Text to be displayed on hover")]
		public string Text;
		[Tooltip("The text field in which to display the text")]
		public Text TextField;
		
		public void OnPointerEnter(PointerEventData data) {
			TextField.text = Text;
		}
		
	}
}
