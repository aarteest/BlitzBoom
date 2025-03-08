using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleRespawnWaypoints : MonoBehaviour
{
	[SerializeField] private LayerMask drivableLayer;
	[SerializeField] private Transform[] respawnPoints; // Assign in Inspector
	[SerializeField] private float checkRadius = 3f;
	[SerializeField] private float offTrackTimeThreshold = 3f; // 3 sec off-track time
	[SerializeField] private float groundCheckDistance = 5f; // Raycast downward distance
	[SerializeField] private float respawnYOffset = 1f; // Small buffer to prevent clipping

	private Transform lastPassedRespawnPoint; // Tracks the last checkpoint/respawn point
	private float offTrackTimer = 0f;

	private void Start()
	{
		if (respawnPoints.Length > 0)
			lastPassedRespawnPoint = respawnPoints[0]; // Default to first respawn point
	}

	private void Update()
	{
		if (IsOnDrivableSurface())
		{
			offTrackTimer = 0f;
			UpdateLastPassedRespawnPoint();
		}
		else
		{
			offTrackTimer += Time.deltaTime;
			if (offTrackTimer >= offTrackTimeThreshold)
			{
				RespawnAtNearestOrLastPassedPoint();
				offTrackTimer = 0f;
			}
		}
	}

	private bool IsOnDrivableSurface()
	{
		return Physics.OverlapSphere(transform.position, checkRadius, drivableLayer).Length > 0;
	}

	private void UpdateLastPassedRespawnPoint()
	{
		Transform nearest = GetNearestRespawnPoint();
		if (nearest != null)
			lastPassedRespawnPoint = nearest;
	}

	private Transform GetNearestRespawnPoint()
	{
		if (respawnPoints.Length == 0)
			return null;

		Transform closestPoint = null;
		float minDistance = Mathf.Infinity;

		foreach (Transform point in respawnPoints)
		{
			float distance = Vector3.Distance(transform.position, point.position);
			if (distance < minDistance)
			{
				minDistance = distance;
				closestPoint = point;
			}
		}

		return closestPoint;
	}

	private void RespawnAtNearestOrLastPassedPoint()
	{
		Transform respawnPoint = GetNearestRespawnPoint();
		if (respawnPoint == null) respawnPoint = lastPassedRespawnPoint;

		if (respawnPoint != null)
		{
			Vector3 newPosition = GetGroundPosition(respawnPoint.position);
			transform.position = newPosition;
			transform.rotation = respawnPoint.rotation;

			Debug.Log("Respawned at checkpoint: " + respawnPoint.name);
		}
		else
		{
			Debug.LogError("No valid respawn point available! Vehicle is stuck.");
		}
	}

	private Vector3 GetGroundPosition(Vector3 originalPosition)
	{
		RaycastHit hit;
		if (Physics.Raycast(originalPosition + Vector3.up * groundCheckDistance, Vector3.down, out hit, groundCheckDistance * 2, drivableLayer))
		{
			return hit.point + Vector3.up * respawnYOffset; // Adjusts position to avoid clipping
		}
		return originalPosition; // If no ground found, default to original position
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Checkpoint"))
		{
			lastPassedRespawnPoint = other.transform;
			Debug.Log("Checkpoint Passed: " + other.gameObject.name);
		}
	}
}
