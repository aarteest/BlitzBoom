using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReboundAbility : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 30f;             // Speed during the dash
    public int maxDashMoves = 3;             // Maximum number of dashes
    public float detectionRange = 50f;       // Range to detect wallLayer objects
    public LayerMask wallLayer;              // Layer mask for detecting walls
    public float stopDistance = 1.5f;        // Distance to maintain from the collider
    public float offsetDistance = 0.5f;      // Offset after reaching the target to avoid overlap

    private Rigidbody rb;
    private int currentMoveCount = 0;
    private bool isDashing = false;
    private Transform targetPoint;

    private AudioManager audioManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on the car.");
        }

		// Get the AudioManager instance
		audioManager = AudioManager.Instance;
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && !isDashing) // Press 'E' to activate dash ability
        {
            StartDash();
        }
    }

    void FixedUpdate()
    {
        if (isDashing && targetPoint != null)
        {
            MoveTowardsTarget();

            float distanceToTarget = Vector3.Distance(transform.position, targetPoint.position);
            if (distanceToTarget <= stopDistance)
            {
                HandleTargetReached();
            }
        }
    }

    void StartDash()
    {
        isDashing = true;
        currentMoveCount = 0;
        FindNextTarget();

		// Play the dash ability sound
		audioManager.PlayAbilitySound(3);
	}

    void StopDash()
    {
        isDashing = false;

        // Maintain forward velocity after the last bounce
        Vector3 forwardVelocity = transform.forward * dashSpeed;
        rb.velocity = forwardVelocity;

        targetPoint = null; // Clear target

		// Stop the dash ability sound
		audioManager.StopAbilitySound();
	}

    void MoveTowardsTarget()
    {
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        rb.velocity = direction * dashSpeed;
    }

    void HandleTargetReached()
    {
        currentMoveCount++;

        // Slightly offset the car to avoid overlap
        Vector3 offsetDirection = (transform.position - targetPoint.position).normalized;
        transform.position = targetPoint.position + offsetDirection * offsetDistance;

        if (currentMoveCount >= maxDashMoves)
        {
            StopDash(); // Transition to forward momentum
        }
        else
        {
            FindNextTarget();
        }
    }

    void FindNextTarget()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRange, wallLayer);

        Transform closestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (var collider in colliders)
        {
            Vector3 toCollider = collider.transform.position - transform.position;

            // Ensure the collider is in the forward direction
            if (Vector3.Dot(transform.forward, toCollider.normalized) > 0)
            {
                float distance = toCollider.magnitude;
                if (distance < closestDistance && distance > stopDistance)
                {
                    closestDistance = distance;
                    closestTarget = collider.transform;
                }
            }
        }

        targetPoint = closestTarget;

        if (targetPoint == null)
        {
            Debug.Log("No valid target found in the forward direction.");
            StopDash(); // Stop dashing if no target is found
        }
    }

    // Gizmo to visualize the detection range in the Scene view
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green; // Set gizmo color to green
        Gizmos.DrawWireSphere(transform.position, detectionRange); // Draw wireframe sphere

        if (targetPoint != null)
        {
            Gizmos.color = Color.red; // Set gizmo color to red
            Gizmos.DrawLine(transform.position, targetPoint.position); // Draw line to target
        }
    }



}
