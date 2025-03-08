using UnityEngine;

public class BouncyCarController : MonoBehaviour
{
    [Header("Car Components")]
    public Transform[] wheels;  // Array of wheel transforms (left front, right front, left rear, right rear)
    public WheelCollider[] wheelColliders;  // Array of wheel colliders
    public Rigidbody rb;

    [Header("Movement Parameters")]
    public float acceleration = 2000f;
    public float turnSpeed = 500f;

    [Header("Spring Joints (Suspension)")]
    public float springForce = 1000f;
    public float damperForce = 100f;
    public float suspensionDistance = 0.2f;

    [Header("Bounciness Parameters")]
    public float bounceForce = 300f;  // How much the car bounces
    public float bounceDamping = 1f;  // How fast the bounce effect stops (damping)

    private float currentBounceForce = 0f;

    void Start()
    {
        if (!rb)
        {
            rb = GetComponent<Rigidbody>();
        }

        SetupWheels();
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleSuspension();
        HandleBounciness();
    }

    void SetupWheels()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            // Create a spring joint for each wheel to simulate suspension
            SpringJoint springJoint = wheels[i].gameObject.AddComponent<SpringJoint>();
            springJoint.connectedBody = rb;
            springJoint.spring = springForce;
            springJoint.damper = damperForce;
            springJoint.maxDistance = suspensionDistance;
            springJoint.enableCollision = false;
        }
    }

    void HandleMovement()
    {
        float moveInput = Input.GetAxis("Vertical");  // Forward/Back input (W/S or Arrow keys)
        float turnInput = Input.GetAxis("Horizontal");  // Left/Right input (A/D or Arrow keys)

        // Calculate the movement force
        Vector3 force = transform.forward * moveInput * acceleration;
        rb.AddForce(force);

        // Calculate the turning force (rotate around the Y axis)
        float turn = turnInput * turnSpeed * Time.fixedDeltaTime;
        rb.AddTorque(Vector3.up * turn);
    }

    void HandleSuspension()
    {
        // Adjust wheel colliders for suspension behavior (height and spring)
        for (int i = 0; i < wheelColliders.Length; i++)
        {
            WheelCollider wheel = wheelColliders[i];

            // Update wheel position and rotation based on wheel collider
            wheel.GetWorldPose(out Vector3 wheelPos, out Quaternion wheelRot);
            wheels[i].position = wheelPos;
            wheels[i].rotation = wheelRot;
        }
    }

    void HandleBounciness()
    {
        // Simulate the bounce by using the Rigidbody's velocity
        if (Mathf.Abs(rb.velocity.y) < 0.1f)  // When the car is near the ground
        {
            currentBounceForce = Mathf.Lerp(currentBounceForce, bounceForce, bounceDamping * Time.fixedDeltaTime);
        }
        else
        {
            currentBounceForce = Mathf.Lerp(currentBounceForce, 0f, bounceDamping * Time.fixedDeltaTime);
        }

        // Apply a vertical force to simulate bounce
        if (rb.velocity.y < 0)  // Only apply bounce if falling downwards
        {
            rb.AddForce(Vector3.up * currentBounceForce, ForceMode.Impulse);
        }
    }

    // Draw gizmos to visualize the suspension (Optional)
    void OnDrawGizmos()
    {
        if (wheels != null && wheels.Length > 0)
        {
            foreach (var wheel in wheels)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(wheel.position, 0.1f);  // Draw a sphere at each wheel position
            }
        }
    }
}