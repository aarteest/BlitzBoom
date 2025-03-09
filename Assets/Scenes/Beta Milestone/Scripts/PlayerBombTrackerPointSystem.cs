using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem.iOS;

public class PlayerBombTrackerPointSystem : NetworkBehaviour
{
	public TextMeshProUGUI pointsText; // Assign this in the inspector
	private int totalPoints = 0;
	[SerializeField] private int pointsPerSecond = 5;
	private float bombHoldTime = 0f;
	private bool hasBomb = false;
	private float timeToAddPoints = 1f;
	private float previousPointsTime;
	public TextMeshProUGUI currentLapText;
	public GameObject leaderBoardTextPrefab; 


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

				if (currentLap >= maxLapCount)
				{
					CheckpointManager.instance.AddPlayerToLeaderboard(this, totalPoints);
				}

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

	}
}
