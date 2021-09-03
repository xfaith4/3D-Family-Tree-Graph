using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera : MonoBehaviour
{
    public GameObject graph;
    public float speed = 5.0f;

    void Start() { }

    void Update()
    {
        if (Input.GetKey("up"))
        {
            transform.position += new Vector3(0, 0, Time.deltaTime * speed);
        }
        if (Input.GetKey("down"))
        {
            transform.position -= new Vector3(0, 0, Time.deltaTime * speed);
        }
        if (Input.GetKey("left"))
        {
            transform.position += new Vector3(Time.deltaTime * speed, 0, 0);
        }
        if (Input.GetKey("right"))
        {
            transform.position -= new Vector3(Time.deltaTime * speed, 0, 0);
        }
    }
}
