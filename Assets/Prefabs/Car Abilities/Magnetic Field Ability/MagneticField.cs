using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class MagneticField : MonoBehaviour
{
	public float magneticRadius = 10f;      // Radius of the magnetic field
	public float pushForce = 500f;         // Strength of the push force
	public float cooldownTime = 5f;        // Cooldown time between uses
	public LayerMask debrisLayer;          // Layer mask for debris/obstacles
	public LayerMask groundLayer;           // Layer mask for the track surface

	private float lastUsedTime = -Mathf.Infinity; // Last time the magnetic field was used
	private AudioManager audioManager;

	void Start()
	{
		// Get the AudioManager instance
		audioManager = AudioManager.Instance;
	}

	void Update()
	{
		// Check if the Control key is pressed and the ability is off cooldown
		if (Input.GetKeyDown(KeyCode.LeftControl) && Time.time >= lastUsedTime + cooldownTime)
		{
			ActivateMagneticField();
		}
	}

	void ActivateMagneticField()
	{
		// Set the cooldown
		lastUsedTime = Time.time;

		// Find all debris in the magnetic field radius
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, magneticRadius, debrisLayer);

		foreach (Collider collider in hitColliders)
		{
			// Calculate the nearest center point on the track
			Vector3 trackCenter = GetTrackCenter(transform.position);

			// Calculate the direction to push the debris (away from the track center)
			Vector3 pushDirection = (collider.transform.position - trackCenter).normalized;

			// Apply a force to the debris
			if (collider.attachedRigidbody != null)
			{
				collider.attachedRigidbody.AddForce(pushDirection * pushForce, ForceMode.Impulse);
			}
		}

		// Play the magnetic field activation sound
		audioManager.PlayAbilitySound(0);

		// Optional: Add a visual or audio effect when the magnetic field activates
		StartCoroutine(ShowMagneticEffect());
	}

	Vector3 GetTrackCenter(Vector3 position)
	{
		RaycastHit hit;

		// Cast a ray downwards to find the track surface
		if (Physics.Raycast(position + Vector3.up * 5f, Vector3.down, out hit, 10f, groundLayer))
		{
			Vector3 trackPoint = hit.point; // The exact point on the track

			// Cast rays to the left and right to find the track edges
			Vector3 leftEdge = trackPoint;
			Vector3 rightEdge = trackPoint;

			if (Physics.Raycast(trackPoint + Vector3.left * 10f, Vector3.down, out hit, 20f, groundLayer))
			{
				leftEdge = hit.point;
			}
			if (Physics.Raycast(trackPoint + Vector3.right * 10f, Vector3.down, out hit, 20f, groundLayer))
			{
				rightEdge = hit.point;
			}

			// Calculate the midpoint between left and right edges
			Vector3 trackCenter = (leftEdge + rightEdge) / 2f;
			return trackCenter;
		}

		return position; // If no track is detected, return original position
	}

	IEnumerator ShowMagneticEffect()
	{
		// Placeholder for a visual/audio effect (e.g., particle system or UI feedback)
		Debug.Log("Magnetic field activated!");
		yield return new WaitForSeconds(1f);

		// Stop the sound after the effect has completed
		audioManager.StopAbilitySound();
	}

	void OnDrawGizmosSelected()
	{
		// Draw the magnetic field radius in the Scene view for debugging
		Gizmos.color = Color.cyan;
		Gizmos.DrawWireSphere(transform.position, magneticRadius);
	}
}
