using Cinemachine;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class MultiplayerCarController : NetworkBehaviour
{
    [Header("Hover Settings")]
    public Transform[] hoverPoints;
    public float hoverHeight = 2f;
    public float hoverForce = 10f;
    public float damping = 5f;
    public LayerMask groundLayer;

    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float reverseSpeed = 5f;
    public float coastDamping = 0.5f;

    [Header("Turning Settings")]
    public Transform leftThruster;
    public Transform rightThruster;
    public float turnTorque = 50f;
    public float turnSpeedMultiplier = 2f;
    public float maxTurningForce = 100f;
    public float turnDampeningAtSpeed = 0.5f;

    [Header("Banking Settings")]
    public float bankAngleLimit = 30f;
    private float currentBankAngle = 0f;

    [Header("Collision Settings")]
    public float collisionDamping = 0.5f;
    public float collisionDrag = 5f;

    [Header("Boost Settings")]
    public float maxBoostAmount = 100f; // Maximum boost value
    public float currentBoostAmount;    // Current boost value
    public Slider boostSlider;          // Reference to the UI Slider
    public float boostMultiplier = 1.5f; // Speed multiplier during boost
    public float boostDuration = 1.5f;  // Duration of boost in seconds
    public float boostCooldown = 3f;    // Cooldown time for the boost
    public float boostFOVIncrease = 10f; // Additional FOV when boosting
    public float fovTransitionSpeed = 5f; // Speed of FOV adjustment
    public CinemachineVirtualCamera virtualCamera;
    public float maxBoostSpeed = 50f;  // Maximum speed during boost

    [Header("Other Settings")]
    public float handbrakeDrag = 2f;

    private Rigidbody rb;
    private float originalDrag;
    private float defaultFOV;
    private bool isBoosting = false;
    private bool boostOnCooldown = false;
    private float boostTimer = 0f;
    private float cooldownTimer = 0f;

    public Slider Boostmeter; // Reference to the Boost Meter UI Slider

    [SerializeField] private Camera mainCam;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalDrag = rb.drag;

        Debug.Log($"IsServer: {IsServer}, IsClient: {IsClient}, IsOwner: {IsOwner}");

        CinemachineBrain brain = mainCam.GetComponent<CinemachineBrain>();

        if (!IsOwner) return;

        if (IsOwner)
        {
            if (brain != null)
            {

                virtualCamera.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Cinemachine Brain not found on the main camera.");
            }
        }

        // Store the initial FOV of the virtual camera
        defaultFOV = virtualCamera.m_Lens.FieldOfView;

        // Initialize boost values
        currentBoostAmount = maxBoostAmount;

        if (boostSlider != null)
        {
            boostSlider.maxValue = maxBoostAmount;
            boostSlider.value = currentBoostAmount;
        }



    }

    private void Update()
    {
        ApplyHoverForce();
        AlignWithSurface();

        if (!IsOwner) return;

        if (IsOwner)
        {

            // Get input locally
            float moveInput = Input.GetAxis("Vertical");
            float turnInput = Input.GetAxis("Horizontal");
            bool boostInput = Input.GetButton("Boost");


            if (moveInput != 0 || turnInput != 0)
            {
                Debug.Log($"Client Attempting ServerRpc: moveInput={moveInput}, turnInput={turnInput}");
                SubmitMovementServerRpc(moveInput, turnInput, boostInput);
            }

        }



    }

    [ServerRpc(RequireOwnership = false)] // Allow any client to call this ServerRpc
    public void SubmitMovementServerRpc(float moveInput, float turnInput, bool boostInput)
    {
        Debug.Log($"ServerRpc called: moveInput={moveInput}, turnInput={turnInput}");

        // Optional: Call movement handling
        HandleMovement(moveInput);
        ApplyTurnForce(turnInput);
        HandleBoost(boostInput);
    }

    //private void Update()
    //{
    //    ApplyHoverForce();
    //    HandleMovement();
    //    ApplyTurnForce();
    //    ApplyBanking();
    //    AlignWithSurface();
    //    HandleBoost();
    //}



    void ApplyHoverForce()
    {
        foreach (Transform hoverPoint in hoverPoints)
        {
            if (Physics.Raycast(hoverPoint.position, Vector3.down, out RaycastHit hit, hoverHeight * 2f, groundLayer))
            {
                float distanceToGround = hit.distance;
                float hoverError = hoverHeight - distanceToGround;
                float upwardForce = hoverForce * hoverError - rb.velocity.y * damping;

                rb.AddForceAtPosition(Vector3.up * upwardForce, hoverPoint.position, ForceMode.Acceleration);
            }
        }
    }

    void HandleMovement(float moveInput)
    {
        float moveInputValue = Input.GetAxis("Vertical");
        float currentSpeed = moveSpeed;

        if (isBoosting)
        {
            currentSpeed *= boostMultiplier;
        }

        if (moveInput > 0)
        {
            rb.AddForce(transform.forward * moveInput * currentSpeed, ForceMode.Acceleration);
        }
        else if (moveInput < 0)
        {
            rb.AddForce(transform.forward * moveInput * reverseSpeed, ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(-rb.velocity.normalized * coastDamping, ForceMode.Acceleration);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            rb.drag = handbrakeDrag;
        }
        else
        {
            rb.drag = originalDrag;
        }
    }

    void ApplyTurnForce(float turnInput)
    {
        float turnInputValue = Input.GetAxis("Horizontal");

        ApplyBanking();

        if (Mathf.Abs(turnInput) > 0.01f)
        {
            float speedFactor = Mathf.Clamp01(rb.velocity.magnitude / 10f);
            float adjustedTurnTorque = turnTorque * (1f - (turnDampeningAtSpeed * speedFactor)) * turnSpeedMultiplier;

            rb.AddForceAtPosition(
                turnInput > 0 ? -transform.up * adjustedTurnTorque * turnInput : transform.up * adjustedTurnTorque * -turnInput,
                leftThruster.position,
                ForceMode.Force
            );
            rb.AddForceAtPosition(
                turnInput > 0 ? transform.up * adjustedTurnTorque * turnInput : -transform.up * adjustedTurnTorque * -turnInput,
                rightThruster.position,
                ForceMode.Force
            );
        }
    }

    void ApplyBanking()
    {
        float targetBankAngle = Mathf.Lerp(bankAngleLimit, -bankAngleLimit, (Input.GetAxis("Horizontal") + 1f) / 2f);
        currentBankAngle = Mathf.LerpAngle(currentBankAngle, targetBankAngle, Time.deltaTime * 8f);

        transform.localRotation = Quaternion.Euler(
            transform.localRotation.eulerAngles.x,
            transform.localRotation.eulerAngles.y,
            currentBankAngle
        );
    }

    void AlignWithSurface()
    {
		RaycastHit hitFront, hitBack;
		Vector3 frontPoint = transform.position + transform.forward * 1.5f; // Adjust as needed
		Vector3 backPoint = transform.position - transform.forward * 1.5f;

		bool frontHit = Physics.Raycast(frontPoint, Vector3.down, out hitFront, hoverHeight * 2f, groundLayer);
		bool backHit = Physics.Raycast(backPoint, Vector3.down, out hitBack, hoverHeight * 2f, groundLayer);

		if (frontHit && backHit)
		{
			// Get the ground normal at the mid-point
			Vector3 groundNormal = (hitFront.normal + hitBack.normal).normalized;

			// Compute the new rotation aligning the car with the slope
			Quaternion targetRotation = Quaternion.FromToRotation(transform.up, groundNormal) * transform.rotation;

			// Smoothly adjust rotation
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
		}
	}

    void HandleBoost(bool boostInputValue)
    {
        if (Input.GetButton("Boost") && currentBoostAmount > 0f)
        {
            isBoosting = true;

            currentBoostAmount -= Time.deltaTime * (maxBoostAmount / boostDuration);
            boostSlider.value = currentBoostAmount;

            if (rb.velocity.magnitude < maxBoostSpeed)
            {
                rb.AddForce(transform.forward * moveSpeed * boostMultiplier, ForceMode.Acceleration);
            }

            float targetFOV = defaultFOV + boostFOVIncrease;
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
        }
        else
        {
            isBoosting = false;
        }

        if (currentBoostAmount <= 0f)
        {
            currentBoostAmount = 0f;
            boostOnCooldown = true;
        }

        if (boostOnCooldown)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= boostCooldown)
            {
                boostOnCooldown = false;
                cooldownTimer = 0f;
                currentBoostAmount = maxBoostAmount;
                boostSlider.value = currentBoostAmount;
            }
        }

        if (!isBoosting && rb.velocity.magnitude > moveSpeed)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, rb.velocity.normalized * moveSpeed, Time.deltaTime * 2f);
        }

        if (!isBoosting)
        {
            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, defaultFOV, Time.deltaTime * fovTransitionSpeed * 0.5f);
        }
    }

    public float GetCurrentSpeed()
    {
        return rb.velocity.magnitude * 3.6f;
    }

}














//using Cinemachine;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using System;
//using System.Reflection;
//using Unity.Netcode;
//using UnityEditor;


//public class MultiplayerCarController : NetworkBehaviour
//{
//    [Header("Hover Settings")]
//    public Transform[] hoverPoints;
//    public float hoverHeight = 2f;
//    public float hoverForce = 10f;
//    public float damping = 5f;
//    public LayerMask groundLayer;

//    [Header("Movement Settings")]
//    public float moveSpeed = 10f;
//    public float reverseSpeed = 5f;
//    public float coastDamping = 0.5f;

//    [Header("Turning Settings")]
//    public Transform leftThruster;
//    public Transform rightThruster;
//    public float turnTorque = 50f;
//    public float turnSpeedMultiplier = 2f;
//    public float turnDampeningAtSpeed = 0.5f;

//    [Header("Boost Settings")]
//    public float maxBoostAmount = 100f;
//    public float currentBoostAmount;
//    public Slider boostSlider;
//    public float boostMultiplier = 1.5f;
//    public float boostDuration = 1.5f;
//    public float boostCooldown = 3f;

//    private Rigidbody rb;
//    private bool isBoosting = false;
//    private bool boostOnCooldown = false;
//    private float cooldownTimer = 0f;

//    [SerializeField] private CinemachineVirtualCamera virtualCamera;

//    private void Start()
//    {
//        rb = GetComponent<Rigidbody>();
//        if (IsOwner)
//        {
//            virtualCamera.gameObject.SetActive(true);
//        }
//    }

//    private void Update()
//    {
//        if (!IsOwner) return;

//        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
//        bool boostInput = Input.GetButton("Boost");
//        SubmitMovementServerRpc(moveInput, boostInput);
//    }

//    [ServerRpc]
//    private void SubmitMovementServerRpc(Vector2 moveInput, bool boostInput)
//    {
//        HandleMovement(moveInput, boostInput);
//    }

//    private void HandleMovement(Vector2 moveInput, bool boostInput)
//    {
//        ApplyHoverForce();
//        ApplyTurnForce(moveInput.x);
//        AlignWithSurface();
//        ProcessBoost(boostInput);
//        ProcessAcceleration(moveInput.y);
//    }

//    private void ApplyHoverForce()
//    {
//        foreach (Transform hoverPoint in hoverPoints)
//        {
//            if (Physics.Raycast(hoverPoint.position, Vector3.down, out RaycastHit hit, hoverHeight * 2f, groundLayer))
//            {
//                float hoverError = hoverHeight - hit.distance;
//                float upwardForce = hoverForce * hoverError - rb.velocity.y * damping;
//                rb.AddForceAtPosition(Vector3.up * upwardForce, hoverPoint.position, ForceMode.Acceleration);
//            }
//        }
//    }

//    private void ApplyTurnForce(float turnInput)
//    {
//        if (Mathf.Abs(turnInput) < 0.01f) return;
//        float speedFactor = Mathf.Clamp01(rb.velocity.magnitude / 10f);
//        float adjustedTurnTorque = turnTorque * (1f - (turnDampeningAtSpeed * speedFactor)) * turnSpeedMultiplier;

//        rb.AddForceAtPosition(turnInput > 0 ? -transform.up * adjustedTurnTorque : transform.up * adjustedTurnTorque,
//                              leftThruster.position, ForceMode.Force);
//        rb.AddForceAtPosition(turnInput > 0 ? transform.up * adjustedTurnTorque : -transform.up * adjustedTurnTorque,
//                              rightThruster.position, ForceMode.Force);
//    }

//    private void AlignWithSurface()
//    {
//        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, hoverHeight * 2f, groundLayer))
//        {
//            Vector3 surfaceNormal = hit.normal;
//            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;
//            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
//        }
//    }

//    private void ProcessBoost(bool boostInput)
//    {
//        if (boostInput && currentBoostAmount > 0f)
//        {
//            isBoosting = true;
//            currentBoostAmount -= Time.deltaTime * (maxBoostAmount / boostDuration);
//            boostSlider.value = currentBoostAmount;
//            rb.AddForce(transform.forward * moveSpeed * boostMultiplier, ForceMode.Acceleration);
//        }
//        else
//        {
//            isBoosting = false;
//        }

//        if (currentBoostAmount <= 0f)
//        {
//            boostOnCooldown = true;
//        }

//        if (boostOnCooldown)
//        {
//            cooldownTimer += Time.deltaTime;
//            if (cooldownTimer >= boostCooldown)
//            {
//                boostOnCooldown = false;
//                cooldownTimer = 0f;
//                currentBoostAmount = maxBoostAmount;
//                boostSlider.value = currentBoostAmount;
//            }
//        }
//    }

//    private void ProcessAcceleration(float moveInput)
//    {
//        float speed = isBoosting ? moveSpeed * boostMultiplier : moveSpeed;
//        rb.AddForce(transform.forward * moveInput * (moveInput > 0 ? speed : reverseSpeed), ForceMode.Acceleration);
//    }
//}





//using Cinemachine;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class MultiplayerCarController : MonoBehaviour
//{
//    [Header("Hover Settings")]
//    public Transform[] hoverPoints;
//    public float hoverHeight = 2f;
//    public float hoverForce = 10f;
//    public float damping = 5f;
//    public LayerMask groundLayer;

//    [Header("Movement Settings")]
//    public float moveSpeed = 10f;
//    public float reverseSpeed = 5f;
//    public float coastDamping = 0.5f;

//    [Header("Turning Settings")]
//    public Transform leftThruster;
//    public Transform rightThruster;
//    public float turnTorque = 50f;
//    public float turnSpeedMultiplier = 2f;
//    public float maxTurningForce = 100f;
//    public float turnDampeningAtSpeed = 0.5f;

//    [Header("Banking Settings")]
//    public float bankAngleLimit = 30f;
//    private float currentBankAngle = 0f;

//    [Header("Collision Settings")]
//    public float collisionDamping = 0.5f;
//    public float collisionDrag = 5f;

//    [Header("Boost Settings")]
//    public float maxBoostAmount = 100f; // Maximum boost value
//    public float currentBoostAmount;    // Current boost value
//    public Slider boostSlider;          // Reference to the UI Slider
//    public float boostMultiplier = 1.5f; // Speed multiplier during boost
//    public float boostDuration = 1.5f;  // Duration of boost in seconds
//    public float boostCooldown = 3f;    // Cooldown time for the boost
//    public float boostFOVIncrease = 10f; // Additional FOV when boosting
//    public float fovTransitionSpeed = 5f; // Speed of FOV adjustment
//    public CinemachineVirtualCamera virtualCamera;
//    public float maxBoostSpeed = 50f;  // Maximum speed during boost

//    [Header("Other Settings")]
//    public float handbrakeDrag = 2f;

//    private Rigidbody rb;
//    private float originalDrag;
//    private float defaultFOV;
//    private bool isBoosting = false;
//    private bool boostOnCooldown = false;
//    private float boostTimer = 0f;
//    private float cooldownTimer = 0f;

//    public Slider Boostmeter; // Reference to the Boost Meter UI Slider

//    private void Start()
//    {
//        rb = GetComponent<Rigidbody>();
//        originalDrag = rb.drag;

//        if (virtualCamera == null)
//        {
//            Debug.LogError("Virtual Camera is not assigned. Please assign a Cinemachine Virtual Camera.");
//            return;
//        }

//        // Store the initial FOV of the virtual camera
//        defaultFOV = virtualCamera.m_Lens.FieldOfView;

//        // Initialize boost values
//        currentBoostAmount = maxBoostAmount;

//        if (boostSlider != null)
//        {
//            boostSlider.maxValue = maxBoostAmount;
//            boostSlider.value = currentBoostAmount;
//        }
//    }

//    private void FixedUpdate()
//    {
//        ApplyHoverForce();
//        HandleMovement();
//        ApplyTurnForce();
//        ApplyBanking();
//        AlignWithSurface();
//        HandleBoost();
//    }

//    void ApplyHoverForce()
//    {
//        foreach (Transform hoverPoint in hoverPoints)
//        {
//            if (Physics.Raycast(hoverPoint.position, Vector3.down, out RaycastHit hit, hoverHeight * 2f, groundLayer))
//            {
//                float distanceToGround = hit.distance;
//                float hoverError = hoverHeight - distanceToGround;
//                float upwardForce = hoverForce * hoverError - rb.velocity.y * damping;

//                rb.AddForceAtPosition(Vector3.up * upwardForce, hoverPoint.position, ForceMode.Acceleration);
//            }
//        }
//    }

//    void HandleMovement()
//    {
//        float moveInput = Input.GetAxis("Vertical");
//        float currentSpeed = moveSpeed;

//        if (isBoosting)
//        {
//            currentSpeed *= boostMultiplier;
//        }

//        if (moveInput > 0)
//        {
//            rb.AddForce(transform.forward * moveInput * currentSpeed, ForceMode.Acceleration);
//        }
//        else if (moveInput < 0)
//        {
//            rb.AddForce(transform.forward * moveInput * reverseSpeed, ForceMode.Acceleration);
//        }
//        else
//        {
//            rb.AddForce(-rb.velocity.normalized * coastDamping, ForceMode.Acceleration);
//        }

//        if (Input.GetKey(KeyCode.Space))
//        {
//            rb.drag = handbrakeDrag;
//        }
//        else
//        {
//            rb.drag = originalDrag;
//        }
//    }

//    void ApplyTurnForce()
//    {
//        float turnInput = Input.GetAxis("Horizontal");

//        if (Mathf.Abs(turnInput) > 0.01f)
//        {
//            float speedFactor = Mathf.Clamp01(rb.velocity.magnitude / 10f);
//            float adjustedTurnTorque = turnTorque * (1f - (turnDampeningAtSpeed * speedFactor)) * turnSpeedMultiplier;

//            rb.AddForceAtPosition(
//                turnInput > 0 ? -transform.up * adjustedTurnTorque * turnInput : transform.up * adjustedTurnTorque * -turnInput,
//                leftThruster.position,
//                ForceMode.Force
//            );
//            rb.AddForceAtPosition(
//                turnInput > 0 ? transform.up * adjustedTurnTorque * turnInput : -transform.up * adjustedTurnTorque * -turnInput,
//                rightThruster.position,
//                ForceMode.Force
//            );
//        }
//    }

//    void ApplyBanking()
//    {
//        float targetBankAngle = Mathf.Lerp(bankAngleLimit, -bankAngleLimit, (Input.GetAxis("Horizontal") + 1f) / 2f);
//        currentBankAngle = Mathf.LerpAngle(currentBankAngle, targetBankAngle, Time.deltaTime * 8f);

//        transform.localRotation = Quaternion.Euler(
//            transform.localRotation.eulerAngles.x,
//            transform.localRotation.eulerAngles.y,
//            currentBankAngle
//        );
//    }

//    void AlignWithSurface()
//    {
//        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, hoverHeight * 2f, groundLayer))
//        {
//            Vector3 surfaceNormal = hit.normal;
//            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, surfaceNormal) * transform.rotation;

//            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
//        }
//    }

//    void HandleBoost()
//    {
//        if (Input.GetButton("Boost") && currentBoostAmount > 0f)
//        {
//            isBoosting = true;

//            currentBoostAmount -= Time.deltaTime * (maxBoostAmount / boostDuration);
//            boostSlider.value = currentBoostAmount;

//            if (rb.velocity.magnitude < maxBoostSpeed)
//            {
//                rb.AddForce(transform.forward * moveSpeed * boostMultiplier, ForceMode.Acceleration);
//            }

//            float targetFOV = defaultFOV + boostFOVIncrease;
//            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
//        }
//        else
//        {
//            isBoosting = false;
//        }

//        if (currentBoostAmount <= 0f)
//        {
//            currentBoostAmount = 0f;
//            boostOnCooldown = true;
//        }

//        if (boostOnCooldown)
//        {
//            cooldownTimer += Time.deltaTime;
//            if (cooldownTimer >= boostCooldown)
//            {
//                boostOnCooldown = false;
//                cooldownTimer = 0f;
//                currentBoostAmount = maxBoostAmount;
//                boostSlider.value = currentBoostAmount;
//            }
//        }

//        if (!isBoosting && rb.velocity.magnitude > moveSpeed)
//        {
//            rb.velocity = Vector3.Lerp(rb.velocity, rb.velocity.normalized * moveSpeed, Time.deltaTime * 2f);
//        }

//        if (!isBoosting)
//        {
//            virtualCamera.m_Lens.FieldOfView = Mathf.Lerp(virtualCamera.m_Lens.FieldOfView, defaultFOV, Time.deltaTime * fovTransitionSpeed * 0.5f);
//        }
//    }

//    public float GetCurrentSpeed()
//    {
//        return rb.velocity.magnitude * 3.6f;
//    }

//    public void SlowDown()
//    {
//        // Implement slow-down effect, reducing speed for a period
//        StartCoroutine(SlowDownCoroutine());
//    }

//    private IEnumerator SlowDownCoroutine()
//    {
//        float originalSpeed = moveSpeed;
//        moveSpeed *= 0.7f; // Reduce the car's speed by 70%

//        // Wait for a period (e.g., 3 seconds)
//        yield return new WaitForSeconds(10f);

//        // Restore the original speed after the time is up
//        moveSpeed = originalSpeed;
//    }

//}   