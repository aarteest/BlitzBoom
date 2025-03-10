using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class LeaderBoardManager : MonoBehaviour
{
	[SerializeField] private CharacterDatabase characterData;

	public List<Character> playerScores = new List<Character>();

	public List<Transform> leaderboardPositionTransform = new List<Transform>();
	public List<TextMeshProUGUI> leaderboardTexts = new List<TextMeshProUGUI>();
	private void Start()
	{
		foreach (Character c in characterData.GetAllCharacters())
		{
			if (c.playerScore > 0)
			{
			playerScores.Add(c);

			}
		}

		playerScores = playerScores.OrderByDescending(n => n.playerScore).ToList();

		for (int i = 0; i < 3; i++)
		{
			if (i < playerScores.Count)
			{
			Instantiate(playerScores[i].VictoryPrefab, leaderboardPositionTransform[i]);
			leaderboardTexts[i].text = $"{i+1}) {playerScores[i].DisplayName}: {playerScores[i].playerScore}";

			}
		}
	}


}
