using UnityEngine;
using System.Collections;

public class MouseCast : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		var v3 = Input.mousePosition;
		v3.z = 0.5f;
		v3 = Camera.main.ScreenToWorldPoint(v3);
		transform.position = v3;
	}
}
