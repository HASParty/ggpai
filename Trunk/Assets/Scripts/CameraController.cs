using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.VR;

/// <summary>
/// Uses mouselook if vr is not enabled.
/// </summary>
public class CameraController : MonoBehaviour {

    private MouseLook ml;

	void Start () {
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
