using System.Collections;
using UnityEngine;

public class DebrisBossAbility : MonoBehaviour
{
	public static DebrisBossAbility Instance;

	public GameObject debrisPrefab;
	public int debrisCount = 5;
	public float spawnRadius = 3f;
	public float throwForce = 10f;
	public float cooldownTime = 10f; // Cooldown in seconds (changeable in Inspector)

	public bool debrisThrown = false;

	private bool isOnCooldown = false; // Track cooldown state

	public void SpawnDebris(Vector3 bossPosition, Vector3 bossForward)
	{
		// If on cooldown, do nothing
		//if (isOnCooldown)
		//{
		//	Debug.Log("Debris ability is on cooldown!");
		//	return;
		//}

		

		// Spawn multiple debris objects
		for (int i = 0; i < debrisCount; i++)
		{
			debrisThrown= true;
			Debug.Log("Debris will be spawned and fired");

			// Calculate spawn position behind the boss
			Vector3 spawnOffset = new Vector3(Random.Range(-spawnRadius, spawnRadius), Random.Range(-1f, 1f), 0);
			Vector3 spawnPosition = bossPosition + spawnOffset;

			// Instantiate debris at spawn position
			GameObject debris = Instantiate(debrisPrefab, spawnPosition, Quaternion.identity);

			// Apply force to shoot debris **backward** from the boss
			Rigidbody rb = debris.GetComponent<Rigidbody>();
			if (rb != null)
			{
				// Slightly randomize the backward shot direction (cone effect)
				Vector3 shootDirection = (-bossForward) + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.1f, 0.1f), 0);
				shootDirection.Normalize();

				rb.AddForce(shootDirection * throwForce, ForceMode.Impulse);
			}

			//// Start cooldown coroutine
			//StartCoroutine(StartCooldown());
		}
	}

	// Cooldown coroutine
	private IEnumerator StartCooldown()
	{
		isOnCooldown = true; // Block ability usage
		yield return new WaitForSeconds(cooldownTime); // Wait for cooldown time
		isOnCooldown = false; // Re-enable ability after cooldown
		Debug.Log("Debris ability is ready to use again!");
	}
}








//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class DebrisBossAbility : MonoBehaviour
//{
//	public static DebrisBossAbility Instance; 

//	public GameObject debrisPrefab; // The debris prefab
//	public int debrisCount = 5; // Number of debris to spawn
//	public float spawnRadius = 3f; // Spread of debris
//	public float throwForce = 10f; // Force to apply to debris

//	public float cooldownTime = 10f; // Cooldown duration (changeable in Inspector)
//	private float lastUsedTime = -Mathf.Infinity; // Track last ability use time
//	private bool isFirstTrigger = true; // Allows first activation without cooldown

//	// Method to spawn the debris (called by VritraMovement)
//	public void SpawnDebris(Vector3 bossPosition, Vector3 bossForward)
//	{
//		// Allow first trigger without cooldown
//		if (!isFirstTrigger && Time.time - lastUsedTime < cooldownTime)
//		{
//			Debug.Log("Debris ability on cooldown!");
//			return; // Exit if ability is still on cooldown
//		}

//		// Set first trigger to false after first activation
//		isFirstTrigger = false;
//		lastUsedTime = Time.time;

//		// Spawn multiple debris objects
//		for (int i = 0; i < debrisCount; i++)
//		{
//			Debug.Log("Debris will be spawned and fired");

//			// Calculate spawn position around the boss (spread within a radius)
//			Vector3 spawnOffset = new Vector3(Random.Range(-spawnRadius, spawnRadius), Random.Range(-1f, 1f), 0);
//			Vector3 spawnPosition = bossPosition + spawnOffset;

//			// Instantiate debris at spawn position
//			GameObject debris = Instantiate(debrisPrefab, spawnPosition, Quaternion.identity);

//			// Apply force to shoot debris **backward** from the boss
//			Rigidbody rb = debris.GetComponent<Rigidbody>();
//			if (rb != null)
//			{
//				// Slightly randomize the backward shot direction (cone effect)
//				Vector3 shootDirection = (-bossForward) + new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.1f, 0.1f), 0);
//				shootDirection.Normalize();

//				rb.AddForce(shootDirection * throwForce, ForceMode.Impulse);
//			}
//		}
//	}
//}
