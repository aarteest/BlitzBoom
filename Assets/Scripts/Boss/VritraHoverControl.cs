using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VritraHoverControl : MonoBehaviour
{
	private Rigidbody rb;

	[Header("Hover Settings")]
	public Transform[] hoverPoints; // Points from which the hover force is applied
	public float hoverHeight = 2f;   // Desired height above ground
	public float hoverForce = 10f;   // Upward force applied
	public float damping = 5f;       // Damping to stabilize floating
	public LayerMask groundLayer;    // Layer for ground detection

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		rb.useGravity = false; // Disable gravity so it hovers
	}

	void FixedUpdate()
	{
		ApplyHoverForce();
	}

	void ApplyHoverForce()
	{
		foreach (Transform hoverPoint in hoverPoints)
		{
			RaycastHit hit;
			if (Physics.Raycast(hoverPoint.position, Vector3.down, out hit, hoverHeight * 2, groundLayer))
			{
				float heightDifference = hoverHeight - hit.distance;
				float forceAmount = heightDifference * hoverForce - rb.velocity.y * damping;
				rb.AddForceAtPosition(Vector3.up * forceAmount, hoverPoint.position, ForceMode.Acceleration);
			}
		}
	}
}
