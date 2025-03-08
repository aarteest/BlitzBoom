using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class CheckpointManager : NetworkBehaviour
{
	public static CheckpointManager instance;

	private List<Checkpoints> checkpoints = new List<Checkpoints>();

	[SerializeField] private int lapCount;
	public int maxLapPoints;

	private List<GameObject> totalPlayers = new List<GameObject>();

	public int totalLaps;
	public int pointsPerLap = 10000;

	private void Awake()
	{
		if (instance != null)
		{
			Destroy(this);
		}

		else
		{
			instance = this;
		}
	}

	public override void OnNetworkSpawn()
	{
		StartCoroutine(AddNetworkSpawnDelay());

	}

	private int GetPointsPerLap()
	{
		return pointsPerLap;
	}

	public void LapFinished()
	{
		foreach (Checkpoints c in checkpoints)
		{
			c.GetCrossedCarsList().Clear();
		}

		lapCount++;
	}

	public void AddCheckPoint(Checkpoints newCheckpoint)
	{
		if (!checkpoints.Contains(newCheckpoint))
		{
			checkpoints.Add(newCheckpoint);
		}
	}

	IEnumerator AddNetworkSpawnDelay()
	{
		yield return new WaitForSeconds(0.5f);
		PopulateList();
	}

	private void PopulateList()
	{
		if (IsServer)
		{
			GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); // Ensure all vehicles are tagged properly

			// Only add players that have a BombHolder
			foreach (GameObject player in players)
			{
				if (player.transform.Find("BombHolder") != null)
				{
					totalPlayers.Add(player);
				}
			}

			maxLapPoints = 3 * pointsPerLap;
		}

	}

	public int GetTotalPlayerCount()
	{
		return totalPlayers.Count;
	}


}
