using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barricade : MonoBehaviour
{
    public float speed = 2f; // Speed of movement
    public float maxHeight = 5f; // Maximum height
    public float minHeight = -5f; // Minimum height

    private int direction = 1; // 1 for up, -1 for down

    void Update()
    {
        // Move the object up and down
        transform.position += Vector3.up * speed * direction * Time.deltaTime;

        // Reverse direction when reaching limits
        if (transform.position.y >= maxHeight)
        {
            direction = -1;
        }
        else if (transform.position.y <= minHeight)
        {
            direction = 1;
        }
    }

    void OnDrawGizmos()
    {
        // Set the color of the Gizmos
        Gizmos.color = Color.red;

        // Draw a line showing movement range
        Gizmos.DrawLine(new Vector3(transform.position.x, minHeight, transform.position.z),
                        new Vector3(transform.position.x, maxHeight, transform.position.z));

        // Draw spheres at min and max height
        Gizmos.DrawSphere(new Vector3(transform.position.x, minHeight, transform.position.z), 0.2f);
        Gizmos.DrawSphere(new Vector3(transform.position.x, maxHeight, transform.position.z), 0.2f);
    }
}
