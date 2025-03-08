using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RacingGameSmoothFollow : MonoBehaviour
{
    public Transform carToFollow;
    public Vector3 offset = new Vector3(0, 3, -6);
    public float followSpeed = 10f;
    public float rotationSpeed = 5f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;  // 🚀 Disable gravity (we only use this for smooth movement)
        rb.interpolation = RigidbodyInterpolation.Interpolate; // 🛑 Critical for smooth movement
    }

    void FixedUpdate()
    {
        if (carToFollow == null) return;

        // 🚗 Target position (match car movement)
        Vector3 targetPosition = carToFollow.position + carToFollow.TransformDirection(offset);

        // 🚀 Move smoothly using Rigidbody
        rb.velocity = (targetPosition - transform.position) * followSpeed;

        // 🔄 Smoothly rotate towards car
        Quaternion targetRotation = Quaternion.LookRotation(carToFollow.position - transform.position);
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
    }
}


