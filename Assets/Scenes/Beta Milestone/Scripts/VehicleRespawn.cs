using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleRespawn : MonoBehaviour
{
	private Vector3 lastValidPosition;
	private Quaternion lastValidRotation;
	[SerializeField] private LayerMask drivableLayer;
	[SerializeField] private float checkRadius = 3f;

	private void Update()
	{
		// Check valid positions on the track
		Collider[] colliders = Physics.OverlapSphere(transform.position, checkRadius, drivableLayer);
		if (colliders.Length > 0)
		{
			lastValidPosition = transform.position;
			lastValidRotation = transform.rotation;
		}
	}

	public void HandleExplosion()
	{
		StartCoroutine(RespawnAfterDelay(3f));
	}

	private IEnumerator RespawnAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);

		if (lastValidPosition != Vector3.zero)
		{
			transform.position = lastValidPosition;
			transform.rotation = lastValidRotation;
		}
		else
		{
			Debug.LogWarning("No valid respawn position found!");
		}
	}
}
