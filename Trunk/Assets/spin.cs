using UnityEngine;
using System.Collections;

public class spin : MonoBehaviour
{

    public float speed = 10f;
    void Update()
    {
        transform.Rotate(Vector3.up, speed * Time.deltaTime);
        transform.Rotate(Vector3.right, speed * Time.deltaTime);
        transform.Rotate(Vector3.forward, speed * Time.deltaTime);
    }
}
