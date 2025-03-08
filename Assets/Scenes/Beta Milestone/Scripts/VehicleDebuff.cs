using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleDebuff : MonoBehaviour
{
	public Transform[] blastPoints; // Assign 4 points in Inspector (Front, Back, Left, Right)
	private Rigidbody rb;
	private VehicleRespawn respawnScript;

	[SerializeField]
	private bool isBlasted = false;

	[Header("Explosion Settings")]
	[SerializeField] private float explosionForce = 1000f;
	[SerializeField] private float explosionUpwardModifier = 2f;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		respawnScript = GetComponent<VehicleRespawn>();
	}

	public void ApplyExplosion()
	{
		if (isBlasted) return; // Prevent multiple explosions
		isBlasted = true;

		// Select a random blast direction from the assigned points
		int randomIndex = Random.Range(0, blastPoints.Length);
		Vector3 explosionPosition = blastPoints[randomIndex].position;

		// Apply force away from explosion point
		Vector3 forceDirection = (transform.position - explosionPosition).normalized;
		rb.AddForce(forceDirection * explosionForce + Vector3.up * explosionUpwardModifier, ForceMode.Impulse);

		// Disable controls temporarily
		StartCoroutine(DisableControlsAndRespawn());
	}

	private IEnumerator DisableControlsAndRespawn()
	{
		// Disable movement script (replace `VehicleController` with your movement script name)
		GetComponent<JankyCarControl_Multiplayer>().StopCar();

		// Disable movement script (replace `VehicleController` with your movement script name)
		GetComponent<JankyCarControl_Multiplayer>().enabled = false;

		// Delay to show explosion effect
		yield return new WaitForSeconds(3f);

		// Notify respawn script
		if (respawnScript != null)
		{
			respawnScript.HandleExplosion();
		}

		// Re-enable movement
		GetComponent<JankyCarControl_Multiplayer>().enabled = true;
		isBlasted = false;
	}

	public void ResetCar()
	{
		isBlasted = false;
	}
}
