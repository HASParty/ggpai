using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
public class Asteroid : MonoBehaviour {

    Rigidbody rb;
    SphereCollider sc;

	void Start () {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.2f, 0.2f),
                    Random.Range(-0.5f, 0.5f),
                    ForceMode.Impulse);
        sc = GetComponent<SphereCollider>();
	}
	
	// Update is called once per frame
	void Update () {
        //an example of adding force to a rigid body, happens every frame,
        //results in strange movement
        //can also manually adjust the position rather than making use of rigid body physics,
        //but that will make collisions more confusing
	}
}
