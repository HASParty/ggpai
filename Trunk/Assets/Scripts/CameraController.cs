using UnityEngine;
using System.Collections;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.VR;

public class CameraController : MonoBehaviour {

    private Camera cam;
    private MouseLook ml;

	void Start () {
        cam = GetComponent<Camera>();
        ml = new MouseLook();
        ml.Init(transform.parent, transform);
	}

	// Update is called once per frame
	void Update () {
        if (!Input.GetButton("PauseCam") && !VRSettings.enabled) {
            ml.LookRotation(transform.parent, transform);
        }
	}
}
