using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
public class Asteroid : MonoBehaviour {

    Rigidbody rb;
    SphereCollider sc;
    public Vector3 MaxRotation = new Vector3(20,20,20);
    private Vector3 rotation;

	void Start () {
        rb = GetComponent<Rigidbody>();
        rb.SetDensity(1.1f);
        rb.mass = rb.mass;
        rb.AddForce(Random.Range(-30f, 30f),
                    Random.Range(-12f, 12f),
                    Random.Range(-30f, 30f),
                    ForceMode.Impulse);
        sc = GetComponent<SphereCollider>();
        rotation = new Vector3(Random.Range(-MaxRotation.x, MaxRotation.x),
                               Random.Range(-MaxRotation.y, MaxRotation.y), 
                               Random.Range(-MaxRotation.z, MaxRotation.z));
        transform.Rotate(rotation);
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(rotation * Time.deltaTime);
        //an example of adding force to a rigid body, happens every frame,
        //results in strange movement
        //can also manually adjust the position rather than making use of rigid body physics,
        //but that will make collisions more confusing
	}
}
