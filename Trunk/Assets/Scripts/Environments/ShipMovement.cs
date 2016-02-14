using UnityEngine;
using System.Collections;

public class ShipMovement : MonoBehaviour {
    public Transform target;
    public Vector3 velocity;
    public float damp;
    public float maxSpeed;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, damp, maxSpeed);
	}
}
