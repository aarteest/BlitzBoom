using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaosAbility : MonoBehaviour
{
	private bool isAffected = false;
	private float chaosDuration = 5f; // Duration of effect
	private float radius = 5f; // Radius of random movement
	private float speedFluctuationInterval = 0.5f; // Time between speed changes
	private float angle = 0f;

	private MonoBehaviour carController;
	private Rigidbody rb;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();

		// Detect which car controller script is attached

		////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		// COMMENTED THIS FOR NO ERROR


		//if (TryGetComponent(out ActualFloatingCar multiplayerCar))
		//	carController = multiplayerCar;
		//else if (TryGetComponent(out SinglePlayerCarController singlePlayerCar))
		//	carController = singlePlayerCar;
	}

	public void ActivateChaos()
	{
		if (isAffected) return; // Prevent reactivation while active
		isAffected = true;

		if (carController != null)
			carController.enabled = false; // Disable player input

		StartCoroutine(ChaosEffect());
	}

	private IEnumerator ChaosEffect()
	{
		float elapsedTime = 0f;

		while (elapsedTime < chaosDuration)
		{
			elapsedTime += speedFluctuationInterval;

			// Random speed fluctuation
			float randomSpeed = Random.Range(5f, 25f);

			// Move the car in a circular pattern
			angle += Random.Range(20f, 60f) * Mathf.Deg2Rad;
			Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);

			if (rb != null)
			{
				rb.velocity = transform.forward * randomSpeed + offset;
			}

			yield return new WaitForSeconds(speedFluctuationInterval);
		}

		// Restore control
		if (carController != null)
			carController.enabled = true;

		isAffected = false;
	}
}
