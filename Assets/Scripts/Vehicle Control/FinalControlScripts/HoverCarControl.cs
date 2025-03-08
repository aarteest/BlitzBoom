using UnityEngine;


public class HoverCarControl : MonoBehaviour
{
    [Header("Thrusters")]
    public Transform ThrusterFR, ThrusterFL, ThrusterBR, ThrusterBL; // Assign in Inspector
    [SerializeField]
    private float hoverHeight = 2.0f;
    [SerializeField]
    private float hoverForceMin = 50f; // Minimum hover force (lower for more noticeable drop)
    [SerializeField]
    private float hoverForceMax = 200f; // Maximum hover force (higher for more noticeable rise)
    public LayerMask trackLayer;

    [Header("Movement & Steering")]
    public float acceleration = 100f;
    public float maxSpeed = 200f;
    public float turnSpeed = 50f;
    public float tiltAmount = 15f; // How much the car leans when turning
    public float tiltSpeed = 5f;

    private Rigidbody rb;
    private float smoothHoverForce = 0f;
    private float gravityForce = 9.81f * 5f; // Stronger gravity for stability
    private PIDController pidController = new PIDController(1f, 0.1f, 0.5f); // Use your existing PID values

    public Transform raycastOrigin;
    public float raycastDistance = 5f;
    public float alignmentSpeed = 5f;

    [SerializeField]
    bool isGrounded;

    private float hoverForceOscillationSpeed = 2f; // Speed of oscillation (higher = faster bounce)
    private float hoverForceDamping = 0.5f; // Damping to slow down oscillation over time

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    void FixedUpdate()
    {
        ApplyHoverForce();
        AlignToTrack();
        HandleMovement();
        HandleTilt();
        ApplyGravityIfFloating();
    }

    void ApplyHoverForce()
    {
        isGrounded = false;

        ApplyThrusterForce(ThrusterFR);
        ApplyThrusterForce(ThrusterFL);
        ApplyThrusterForce(ThrusterBR);
        ApplyThrusterForce(ThrusterBL);
    }

    void ApplyThrusterForce(Transform thruster)
    {
        RaycastHit hit;
        if (Physics.Raycast(thruster.position, -transform.up, out hit, hoverHeight, trackLayer))
        {
            isGrounded = true;
            float error = hoverHeight - hit.distance;

            // Oscillate between the min and max values with damping
            float targetHoverForce = Mathf.Lerp(hoverForceMin, hoverForceMax, Mathf.PingPong(Time.time * hoverForceOscillationSpeed, 1f));
            float oscillatingForce = Mathf.Lerp(smoothHoverForce, targetHoverForce, hoverForceDamping * Time.fixedDeltaTime);
            smoothHoverForce = oscillatingForce;

            float forceAmount = pidController.Compute(error, Time.fixedDeltaTime) + oscillatingForce;
            rb.AddForceAtPosition(Vector3.up * forceAmount, thruster.position, ForceMode.Acceleration);
        }
    }

    void AlignToTrack()
    {
        RaycastHit frontHit, backHit, noseHit;
        bool frontGrounded = Physics.Raycast(ThrusterFL.position, -transform.up, out frontHit, hoverHeight, trackLayer) ||
                             Physics.Raycast(ThrusterFR.position, -transform.up, out frontHit, hoverHeight, trackLayer);
        bool backGrounded = Physics.Raycast(ThrusterBL.position, -transform.up, out backHit, hoverHeight, trackLayer) ||
                            Physics.Raycast(ThrusterBR.position, -transform.up, out backHit, hoverHeight, trackLayer);
        bool noseGrounded = Physics.Raycast(raycastOrigin.position + transform.forward * 3f, -Vector3.up, out noseHit, hoverHeight * 3f, trackLayer);

        Vector3 trackUp = transform.up;
        Vector3 trackForward = transform.forward;

        if (frontGrounded && backGrounded)
        {
            isGrounded = true;
            trackUp = (frontHit.normal + backHit.normal).normalized; // Track slope roll
            trackForward = Vector3.Cross(transform.right, trackUp).normalized; // Keep car moving forward correctly
        }

        if (noseGrounded)
        {
            Vector3 noseUp = noseHit.normal;
            float noseAngle = Vector3.SignedAngle(Vector3.up, noseUp, transform.right);
            Quaternion noseRotation = Quaternion.Euler(noseAngle, transform.eulerAngles.y, transform.eulerAngles.z);

            // Apply only X-axis tilt (pitch) from nose detection
            transform.rotation = Quaternion.Slerp(transform.rotation, noseRotation, Time.fixedDeltaTime * alignmentSpeed);
        }

        // Apply only the Z-axis (roll) tilt from the front/back thrusters
        Quaternion targetRotation = Quaternion.LookRotation(trackForward, trackUp);
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * alignmentSpeed));
    }

    void HandleMovement()
    {
        float forwardInput = Input.GetAxis("Vertical");
        float turnInput = Input.GetAxis("Horizontal");

        Vector3 targetVelocity = transform.forward * forwardInput * acceleration;
        rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.fixedDeltaTime * 2f);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);

        float turnFactor = Mathf.Clamp(turnInput, -1f, 1f);
        rb.angularVelocity = Vector3.up * turnFactor * turnSpeed * Time.fixedDeltaTime;
    }

    void HandleTilt()
    {
        float turnInput = Input.GetAxis("Horizontal");
        Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y, -turnInput * tiltAmount);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, tiltSpeed * Time.deltaTime);
    }

    void ApplyGravityIfFloating()
    {

        if (!isGrounded)
        {
            // Gradually increase gravity instead of applying full force instantly
            float fallMultiplier = Mathf.Clamp(rb.velocity.y * 0.1f, -10f, 10f);
            rb.AddForce(Vector3.down * (gravityForce + fallMultiplier), ForceMode.Acceleration);
        }

    }

    // Draw Gizmos for debugging
    void OnDrawGizmos()
    {
        if (ThrusterFR && ThrusterFL && ThrusterBR && ThrusterBL)
        {
            DrawRaycastGizmo(ThrusterFR.position, -transform.up, hoverHeight * 2f, Color.red);
            DrawRaycastGizmo(ThrusterFL.position, -transform.up, hoverHeight * 2f, Color.red);
            DrawRaycastGizmo(ThrusterBR.position, -transform.up, hoverHeight * 2f, Color.red);
            DrawRaycastGizmo(ThrusterBL.position, -transform.up, hoverHeight * 2f, Color.red);
        }

        if (raycastOrigin)
        {
            // Adjusted nose raycast to be a straight line
            Vector3 noseStart = raycastOrigin.position + transform.forward * 3f;
            DrawRaycastGizmo(noseStart, -Vector3.up, hoverHeight * 3f, Color.green);
        }
    }

    void DrawRaycastGizmo(Vector3 start, Vector3 direction, float length, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(start, start + direction * length);
        Gizmos.DrawSphere(start + direction * length, 0.1f);
    }

}




















//public class HoverCarControl : MonoBehaviour
//{
//    [Header("Thrusters")]
//    public Transform ThrusterFR, ThrusterFL, ThrusterBR, ThrusterBL; // Assign in Inspector
//    [SerializeField] 
//    private float hoverHeight = 2.0f;
//    [SerializeField] 
//    private float hoverForce = 100f;
//    public LayerMask trackLayer;

//    [Header("Movement & Steering")]
//    public float acceleration = 100f;
//    public float maxSpeed = 200f;
//    public float turnSpeed = 50f;
//    public float tiltAmount = 15f; // How much the car leans when turning
//    public float tiltSpeed = 5f;

//    private Rigidbody rb;
//    private float smoothHoverForce = 0f;
//    private float gravityForce = 9.81f * 5f; // Stronger gravity for stability
//    private PIDController pidController = new PIDController(1f, 0.1f, 0.5f); // Use your existing PID values

//    public Transform raycastOrigin;
//    public float raycastDistance = 5f;
//    public float alignmentSpeed = 5f;

//    [SerializeField]
//    bool isGrounded;

//    void Start()
//    {
//        rb = GetComponent<Rigidbody>();
//        rb.useGravity = false;
//    }

//    void FixedUpdate()
//    {
//        ApplyHoverForce();
//        AlignToTrack();
//        HandleMovement();
//        HandleTilt();
//        ApplyGravityIfFloating();
//    }

//    void ApplyHoverForce()
//    {
//        isGrounded = false;

//        ApplyThrusterForce(ThrusterFR);
//        ApplyThrusterForce(ThrusterFL);
//        ApplyThrusterForce(ThrusterBR);
//        ApplyThrusterForce(ThrusterBL);
//    }

//    void ApplyThrusterForce(Transform thruster)
//    {
//        RaycastHit hit;
//        if (Physics.Raycast(thruster.position, -transform.up, out hit, hoverHeight, trackLayer))
//        {
//            isGrounded = true;
//            float error = hoverHeight - hit.distance;
//            float forceAmount = pidController.Compute(error, Time.fixedDeltaTime);
//            rb.AddForceAtPosition(Vector3.up * forceAmount, thruster.position, ForceMode.Acceleration);
//        }
//    }

//    void AlignToTrack()
//    {
//        RaycastHit frontHit, backHit, noseHit;
//        bool frontGrounded = Physics.Raycast(ThrusterFL.position, -transform.up, out frontHit, hoverHeight, trackLayer) ||
//                             Physics.Raycast(ThrusterFR.position, -transform.up, out frontHit, hoverHeight, trackLayer);
//        bool backGrounded = Physics.Raycast(ThrusterBL.position, -transform.up, out backHit, hoverHeight, trackLayer) ||
//                            Physics.Raycast(ThrusterBR.position, -transform.up, out backHit, hoverHeight, trackLayer);
//        bool noseGrounded = Physics.Raycast(raycastOrigin.position + transform.forward * 3f, -Vector3.up, out noseHit, hoverHeight * 3f, trackLayer);

//        Vector3 trackUp = transform.up;
//        Vector3 trackForward = transform.forward;

//        if (frontGrounded && backGrounded)
//        {
//            isGrounded = true;
//            trackUp = (frontHit.normal + backHit.normal).normalized; // Track slope roll
//            trackForward = Vector3.Cross(transform.right, trackUp).normalized; // Keep car moving forward correctly
//        }

//        if (noseGrounded)
//        {
//            Vector3 noseUp = noseHit.normal;
//            float noseAngle = Vector3.SignedAngle(Vector3.up, noseUp, transform.right);
//            Quaternion noseRotation = Quaternion.Euler(noseAngle, transform.eulerAngles.y, transform.eulerAngles.z);

//            // Apply only X-axis tilt (pitch) from nose detection
//            transform.rotation = Quaternion.Slerp(transform.rotation, noseRotation, Time.fixedDeltaTime * alignmentSpeed);
//        }

//        // Apply only the Z-axis (roll) tilt from the front/back thrusters
//        Quaternion targetRotation = Quaternion.LookRotation(trackForward, trackUp);
//        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * alignmentSpeed));
//    }





//    void HandleMovement()
//    {
//        float forwardInput = Input.GetAxis("Vertical");
//        float turnInput = Input.GetAxis("Horizontal");

//        Vector3 targetVelocity = transform.forward * forwardInput * acceleration;
//        rb.velocity = Vector3.Lerp(rb.velocity, targetVelocity, Time.fixedDeltaTime * 2f);
//        rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxSpeed);

//        float turnFactor = Mathf.Clamp(turnInput, -1f, 1f);
//        rb.angularVelocity = Vector3.up * turnFactor * turnSpeed * Time.fixedDeltaTime;
//    }

//    void HandleTilt()
//    {
//        float turnInput = Input.GetAxis("Horizontal");
//        Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y, -turnInput * tiltAmount);
//        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, tiltSpeed * Time.deltaTime);
//    }

//    void ApplyGravityIfFloating()
//    {

//        if (!isGrounded)
//        {
//            // Gradually increase gravity instead of applying full force instantly
//            float fallMultiplier = Mathf.Clamp(rb.velocity.y * 0.1f, -10f, 10f);
//            rb.AddForce(Vector3.down * (gravityForce + fallMultiplier), ForceMode.Acceleration);
//        }

//    }

//    // Draw Gizmos for debugging
//    void OnDrawGizmos()
//    {
//        if (ThrusterFR && ThrusterFL && ThrusterBR && ThrusterBL)
//        {
//            DrawRaycastGizmo(ThrusterFR.position, -transform.up, hoverHeight * 2f, Color.red);
//            DrawRaycastGizmo(ThrusterFL.position, -transform.up, hoverHeight * 2f, Color.red);
//            DrawRaycastGizmo(ThrusterBR.position, -transform.up, hoverHeight * 2f, Color.red);
//            DrawRaycastGizmo(ThrusterBL.position, -transform.up, hoverHeight * 2f, Color.red);
//        }

//        if (raycastOrigin)
//        {
//            // Adjusted nose raycast to be a straight line
//            Vector3 noseStart = raycastOrigin.position + transform.forward * 3f;
//            DrawRaycastGizmo(noseStart, -Vector3.up, hoverHeight * 3f, Color.green);
//        }
//    }

//    void DrawRaycastGizmo(Vector3 start, Vector3 direction, float length, Color color)
//    {
//        Gizmos.color = color;
//        Gizmos.DrawLine(start, start + direction * length);
//        Gizmos.DrawSphere(start + direction * length, 0.1f);
//    }

//}
