using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public Vector3 direction = Vector3.forward; // Move forward by default
    public float speed = 5f; // Movement speed

    void Update()
    {
        transform.position += direction.normalized * speed * Time.deltaTime;
    }
}
