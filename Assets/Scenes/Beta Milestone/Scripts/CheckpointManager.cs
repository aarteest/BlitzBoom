using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;
using UnityEngine.SceneManagement;
public class CheckpointManager : NetworkBehaviour
{
	public static CheckpointManager instance;

	private List<Checkpoints> checkpoints = new List<Checkpoints>();
	private Dictionary<PlayerBombTrackerPointSystem, int> leaderBoard = new Dictionary<PlayerBombTrackerPointSystem, int> { };
	private List<PlayerBombTrackerPointSystem> playerScripts = new List<PlayerBombTrackerPointSystem>();

	[SerializeField] private int lapCount;
	[SerializeField] private string winningScene; // Scene to load, assign in the Inspector
	public int maxLapPoints;

	public int totalLaps;
	public int pointsPerLap = 10000;
	public int currentLapPoints;

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

		if (lapCount >= totalLaps)
		{
			SceneManager.LoadScene(winningScene);
		}
		//Debug.Log(lapCount);
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
				if (player.TryGetComponent<PlayerBombTrackerPointSystem>(out PlayerBombTrackerPointSystem playerScript))
				{
					if (!playerScripts.Contains(playerScript))
					{
						playerScripts.Add(playerScript);
					}
				}
			}

			maxLapPoints = 3 * pointsPerLap;
			currentLapPoints = maxLapPoints;
		}

	}

	public int GetTotalPlayerCount()
	{
		return playerScripts.Count;
	}

	public void AddPlayerToLeaderboard(PlayerBombTrackerPointSystem playerScript, int playerScore)
	{
		if (!leaderBoard.ContainsKey(playerScript))
		{
			leaderBoard.Add(playerScript, playerScore);

		}

		foreach (var _playerScript in leaderBoard.Keys.ToList())
		{
			_playerScript.UpdateLeaderboardUI();
		}
	}
	public void UpdateLeaderboardRanking()
	{

	}
	public Dictionary<PlayerBombTrackerPointSystem, int> GetLeaderboard()
	{
		return leaderBoard;
	}
}
