using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerBombTrackerPointSystem : MonoBehaviour
{
	public TextMeshProUGUI pointsText; // Assign this in the inspector
	private int totalPoints = 0;
	private float bombHoldTime = 0f;
	private bool hasBomb = false;

	private void Update()
	{
		if (hasBomb)
		{
			bombHoldTime += Time.deltaTime;
			int newPoints = Mathf.FloorToInt(bombHoldTime * 5); // 5 points per second

			if (newPoints > totalPoints)
			{
				totalPoints = newPoints;
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
	}

	public void AddToTotalPoints(int newPoints)
	{
		totalPoints += newPoints;
		UpdateUI();
	}
}
