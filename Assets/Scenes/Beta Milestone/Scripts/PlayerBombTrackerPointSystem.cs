using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem.iOS;

public class PlayerBombTrackerPointSystem : NetworkBehaviour
{
	public TextMeshProUGUI pointsText; // Assign this in the inspector
	public int totalPoints { get; private set; }
	[SerializeField] private int pointsPerSecond = 5;
	private float bombHoldTime = 0f;
	private bool hasBomb = false;
	private float timeToAddPoints = 1f;
	private float previousPointsTime;
	public TextMeshProUGUI currentLapText;
	public GameObject leaderBoardTextPrefab;
	private string playerName;
	[SerializeField] private GameObject leaaderBoardPanel;

	private List<GameObject> spawnedLeaderboardTexts = new List<GameObject>(); 
	private void Awake()
	{
		totalPoints = 0;

		if(leaaderBoardPanel != null)
		leaaderBoardPanel.SetActive(false);

	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

	}
	private void Update()
	{
		if (hasBomb)
		{
			if (previousPointsTime < timeToAddPoints)
			{
				previousPointsTime += Time.deltaTime;
			}

			else
			{
				previousPointsTime = 0;
				totalPoints += pointsPerSecond;
				UpdateUI();
			}
		}
	}

	public void PickUpBomb()
	{
		hasBomb = true;
	}

	public void DropBomb()
	{
		hasBomb = false;
	}

	public int GetTotalPoints()
	{
		return totalPoints;
	}

	private void UpdateUI()
	{
		if (pointsText != null)
		{
			pointsText.text = totalPoints.ToString();
		}

		if (currentLapText != null)
		{
			if (TryGetComponent<JankyCarControl_Multiplayer>(out JankyCarControl_Multiplayer carScript))
			{
				int maxLapCount = CheckpointManager.instance.totalLaps;
				int currentLap = carScript.GetFinishedLapCount();
				currentLapText.text = $"{currentLap}/ {maxLapCount}";


			}

		}
	}

	public void AddToTotalPoints(int newPoints)
	{
		totalPoints += newPoints;
		UpdateUI();
	}

	public void UpdateLeaderboardUI()
	{
		foreach (GameObject spawnedText in spawnedLeaderboardTexts)
		{
			Destroy(spawnedText);
		}
		spawnedLeaderboardTexts.Clear();
		leaaderBoardPanel.SetActive(true);

		foreach (var key in CheckpointManager.instance.GetLeaderboard().Keys)
		{
			GameObject _leaderBoardText = Instantiate(leaderBoardTextPrefab, leaaderBoardPanel.transform);
			if (_leaderBoardText.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI text))
			{
				text.text = $"{key.playerName}: {CheckpointManager.instance.GetLeaderboard()[key]}";
				spawnedLeaderboardTexts.Add(_leaderBoardText);
			}
		}
	}

	public void SetPlayerString(string _playerName)
	{
		playerName = _playerName;
	}

	public string GetPlayerString()
	{
		return playerName;
	}
}
