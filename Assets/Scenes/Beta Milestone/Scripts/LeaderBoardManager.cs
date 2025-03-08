using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LeaderBoardManager : MonoBehaviour
{
	// Optionally assign players manually; if empty, they'll be fetched automatically
	public List<PlayerBombTrackerPointSystem> players;

	// Prefab for a leaderboard entry (should contain a TextMeshProUGUI component)
	public GameObject leaderboardEntryPrefab;

	// Parent transform for the leaderboard UI (e.g., a panel with a Vertical Layout Group)
	public Transform leaderboardParent;

	// Call this method to update the leaderboard UI
	public void UpdateLeaderboard()
	{
		// If players list is empty, find all PlayerBombTracker components in the scene
		if (players == null || players.Count == 0)
		{
			players = new List<PlayerBombTrackerPointSystem>(FindObjectsOfType<PlayerBombTrackerPointSystem>());
		}

		// Clear any existing leaderboard entries
		foreach (Transform child in leaderboardParent)
		{
			Destroy(child.gameObject);
		}

		// Sort the players by their points in descending order
		List<PlayerBombTrackerPointSystem> sortedPlayers = players.OrderByDescending(p => p.GetTotalPoints()).ToList();

		// Create a leaderboard entry for each player
		for (int i = 0; i < sortedPlayers.Count; i++)
		{
			GameObject entryObj = Instantiate(leaderboardEntryPrefab, leaderboardParent);
			TextMeshProUGUI entryText = entryObj.GetComponent<TextMeshProUGUI>();

			// Choose a rank prefix (using emojis for top 3 if desired)
			string rankPrefix = "";
			if (i == 0)
				rankPrefix = "1st: ";
			else if (i == 1)
				rankPrefix = "2nd: ";
			else if (i == 2)
				rankPrefix = "3rd: ";
			else
				rankPrefix = (i + 1) + "th: ";

			// Get the player's name and points
			string playerName = sortedPlayers[i].gameObject.name;
			int points = sortedPlayers[i].GetTotalPoints();

			// Display the rank, player's name, and their points
			entryText.text = rankPrefix + playerName + " - " + points;
		}
	}
}
