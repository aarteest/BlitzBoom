using Unity.Netcode;
using UnityEngine;

public class MovementScript_2 : NetworkBehaviour
{
    public float moveSpeed = 10f; // Speed of movement
    public float turnSpeed = 50f; // Speed of turning
    private Rigidbody rb;

    private void Start()
    {
        if (IsOwner)
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        // Get input locally
        float moveInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        // Send input to the server
        SubmitMovementServerRpc(moveInput, turnInput);
    }

    [ServerRpc]
    private void SubmitMovementServerRpc(float moveInput, float turnInput)
    {
        HandleMovement(moveInput, turnInput);
    }

    private void HandleMovement(float moveInput, float turnInput)
    {
        if (rb == null) return;

        // Apply movement and rotation
        Vector3 moveForce = transform.forward * moveInput * moveSpeed;
        rb.AddForce(moveForce, ForceMode.Force);

        float turnForce = turnInput * turnSpeed * Time.fixedDeltaTime;
        transform.Rotate(0f, turnForce, 0f);
    }
}









//using System.Globalization;
//using UnityEngine;
//using Unity.Netcode;

//public class MovementScript_2 : NetworkBehaviour
//{

//    [Header("Movement Settings")]
//    public float moveSpeed = 10f; // Speed of forward/backward movement
//    public float turnSpeed = 50f; // Speed of turning

//    private Rigidbody rb;

//    private void Start()
//    {

//        if (IsOwner)
//        {
//            Debug.Log("I own this character!");
//        }
//        else
//        {
//            Debug.Log("I do not own this character.");
//        }


//        rb = GetComponent<Rigidbody>();
//    }

//    private void FixedUpdate()
//    {
//        if (IsOwner)
//        {
//            Debug.Log("I own this character!");
//        }
//        else
//        {
//            Debug.Log("I do not own this character.");
//        }

//        // Ensure only the local player can control their car
//        if (!IsOwner) return;

//        HandleMovement();
//    }

//    void HandleMovement()
//    {
//        // Forward and backward movement
//        float moveInput = Input.GetAxis("Vertical");
//        Vector3 moveForce = transform.forward * moveInput * moveSpeed;
//        rb.AddForce(moveForce, ForceMode.Force);

//        // Turning
//        float turnInput = Input.GetAxis("Horizontal");
//        float turnForce = turnInput * turnSpeed * Time.fixedDeltaTime;
//        transform.Rotate(0f, turnForce, 0f);

//        Debug.Log($"Move Input: {moveInput}, Turn Input: {turnInput}");
//    }

//}