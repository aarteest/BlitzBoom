using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LapSystem : MonoBehaviour
{
    public int totalLaps = 3; // Set in Inspector
    public int totalPoints = 30000; // Set in Inspector
    public TextMeshProUGUI lapCounterText;
    private int currentLapCount = 0;
    private int playerCount;

	private void Start()
	{
		
	}

	private bool raceFinished = false;

    private void OnTriggerEnter(Collider other)
    {
        if (raceFinished) return;

        if (other.CompareTag("Player")) // Ensure players have the "Player" tag
        {
            if (other.gameObject.TryGetComponent<PlayerBombTrackerPointSystem>(out PlayerBombTrackerPointSystem pointScript))
            {
                pointScript.AddToTotalPoints(totalPoints);
                totalPoints -= 10000;

				if (totalPoints <= 0)
                {
                    totalPoints = 30000;
                }

			}

		}
    }}
