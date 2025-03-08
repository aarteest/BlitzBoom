using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SuckBombAbility : NetworkBehaviour
{
	[Header("Ability Settings")]
	[SerializeField] private float abilityRadius = 10f; // Detection radius
	[SerializeField] private float bombStealTime = 5f; // Time to hold "Q" to steal
	[SerializeField] private KeyCode stealBombKey = KeyCode.Q; // Key to activate ability
	[SerializeField] private float transferDuration = 1.5f; // Time for bomb interpolation

	private static List<BombBehaviour> activeStealAttempts = new(); // List of bombs being stolen

	private bool isStealing = false;
	private bool isTransferring = false; // Prevents multiple transfers at once
	private float stealProgress = 0f;
	private BombBehaviour targetBomb = null;

	private void Update()
	{
		if (!IsOwner) return; // Ensure only the local player controls this ability

		// Find the closest vehicle with a bomb in range
		FindTargetBomb();

		if (targetBomb != null && Input.GetKey(stealBombKey) && !isTransferring)
		{
			if (!isStealing)
			{
				isStealing = true;
				stealProgress = 0f;
				Debug.Log("Started stealing the bomb!");

				// If another player is already stealing, explode the bomb
				if (activeStealAttempts.Contains(targetBomb))
				{
					Debug.Log("Multiple players detected! Bomb exploding...");
					targetBomb.Explode();
					ResetStealProgress();
					return;
				}

				// Otherwise, add to the list and continue stealing
				activeStealAttempts.Add(targetBomb);
			}

			stealProgress += Time.deltaTime;

			if (stealProgress >= bombStealTime)
			{
				StartCoroutine(InterpolateBombTransfer(targetBomb, gameObject));
			}
		}
		else if (Input.GetKeyUp(stealBombKey))
		{
			ResetStealProgress();
			Debug.Log("Stopped stealing the bomb!");
		}
	}

	private void FindTargetBomb()
	{
		Collider[] colliders = Physics.OverlapSphere(transform.position, abilityRadius);
		targetBomb = null;

		foreach (Collider col in colliders)
		{
			if (col.CompareTag("Player") && col.gameObject != gameObject)
			{
				Debug.Log("Player With Bomb Found");
				BombBehaviour bomb = col.GetComponentInChildren<BombBehaviour>();
				if (bomb != null)
				{
					targetBomb = bomb;
					return;
				}
			}
		}
	}

	private IEnumerator InterpolateBombTransfer(BombBehaviour bomb, GameObject newHolder)
	{
		if (isTransferring) yield break; // Prevent multiple simultaneous transfers
		isTransferring = true;

		Transform bombTransform = bomb.transform;
		Transform startHolder = bombTransform.parent;
		Transform endHolder = newHolder.transform;

		float elapsedTime = 0f;

		while (elapsedTime < transferDuration)
		{
			bombTransform.position = Vector3.Lerp(startHolder.position, endHolder.position, elapsedTime / transferDuration);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		bomb.TransferToNewHolder(newHolder);
		isTransferring = false;

		// Remove from active steals after successful transfer
		activeStealAttempts.Remove(bomb);

		Debug.Log("Bomb successfully transferred!");
	}

	private void ResetStealProgress()
	{
		if (isStealing && targetBomb != null)
		{
			activeStealAttempts.Remove(targetBomb);
		}

		isStealing = false;
		stealProgress = 0f;
	}




}
