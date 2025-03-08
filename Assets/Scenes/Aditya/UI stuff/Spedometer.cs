using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Spedometer : MonoBehaviour
{
    public Rigidbody carRigidbody; // Attach the car's Rigidbody
    public TextMeshProUGUI speedText; // Drag your TextMeshPro UI element here
    public float maxSpeed = 200f; // Adjust based on your game speed

    void Update()
    {
        //float speed = carRigidbody.velocity.magnitude * 3.6f; // Convert m/s to KM/H
        //speedText.text = Mathf.RoundToInt(speed).ToString(); // Show speed as a whole number

        //// Change color based on speed
        //if (speed < maxSpeed * 0.3f) // Slow Speed (Green)
        //    speedText.color = Color.green;
        //else if (speed < maxSpeed * 0.7f) // Medium Speed (Yellow)
        //    speedText.color = Color.yellow;
        //else // High Speed (Red)
        //    speedText.color = Color.red;
    }
}
