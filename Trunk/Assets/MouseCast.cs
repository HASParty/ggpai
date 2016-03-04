using UnityEngine;
using System.Collections;
using Boardgame;

public class MouseCast : MonoBehaviour {

	PhysicalCell cell; 
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void LateUpdate () {
		transform.position = Camera.main.ViewportToWorldPoint (new Vector3 (0.5f, 0.5f, 0.3f));
		if(cell != null && Input.GetKeyUp(KeyCode.Space)) {
			cell.OnMouseUp();
		}
	}

	void FixedUpdate() {
		Ray ray = Camera.main.ViewportPointToRay (new Vector3 (0.5f, 0.5f, 0.3f));
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit, 3f)) {
			if(cell != null) cell.OnMouseExit();
			var newCell = hit.collider.gameObject.GetComponent<PhysicalCell>();
			if(newCell != null) {
				newCell.OnMouseEnter();
				cell = newCell;
			}
		}
	}
}
