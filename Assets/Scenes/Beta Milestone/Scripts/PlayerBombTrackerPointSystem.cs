using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class PlayerBombTrackerPointSystem : NetworkBehaviour
{
    public TextMeshProUGUI pointsText;
    public TextMeshProUGUI livePointsText;
    public int totalPoints { get; private set; }
    [SerializeField] private int pointsPerSecond = 5;
    private float bombHoldTime = 0f;
    private bool hasBomb = false;
    private float timeToAddPoints = 1f;
    private float previousPointsTime;
    public TextMeshProUGUI currentLapText;
    public GameObject leaderBoardTextPrefab;
    [SerializeField] private GameObject leaderboardTextParentPrefab;
    private string playerName;
    [SerializeField] private GameObject leaderBoardPanel;
    [SerializeField] private List<GameObject> leaderBoardTextParents = new List<GameObject>();
    private List<GameObject> spawnedLeaderboardTexts = new List<GameObject>();
    public Character playerCharacter { get; private set; }
    [SerializeField] private GameObject bombOwnerFeedback;

    private void Awake()
    {
        totalPoints = 0;
        if (leaderBoardPanel != null)
            leaderBoardPanel.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        AddToTotalPointsServerRpc(1);
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
                AddToTotalPointsServerRpc(pointsPerSecond);
            }
            bombOwnerFeedback.SetActive(true);
        }
        else
        {
            bombOwnerFeedback.SetActive(false);
        }
    }

    [ServerRpc]
    public void PickUpBombServerRpc()
    {
        hasBomb = true;
        UpdateBombOwnerClientRpc(true);
    }

    [ServerRpc]
    public void DropBombServerRpc()
    {
        hasBomb = false;
        UpdateBombOwnerClientRpc(false);
    }

    [ClientRpc]
    public void UpdateBombOwnerClientRpc(bool ownsBomb)
    {
        hasBomb = ownsBomb;
        bombOwnerFeedback.SetActive(ownsBomb);
    }

    [ServerRpc]
    public void AddToTotalPointsServerRpc(int newPoints)
    {
        totalPoints += newPoints;
        UpdateUI();
        UpdateLivePointsClientRpc(newPoints);
    }

    [ClientRpc]
    public void UpdateLivePointsClientRpc(int newPoints)
    {
        if (livePointsText != null)
        {
            if (newPoints >= 1000)
                StartCoroutine(ShowLapBonus(newPoints));
            else
                StartCoroutine(ShowLivePointIncrements(newPoints));
        }
    }

    private IEnumerator ShowLivePointIncrements(int pointsToAdd)
    {
        livePointsText.text = $"+{pointsToAdd}";
        yield return new WaitForSeconds(0.2f);
        livePointsText.text = "";
    }

    private IEnumerator ShowLapBonus(int lapBonus)
    {
        livePointsText.text = $"+{lapBonus}";
        yield return new WaitForSeconds(1.5f);
        livePointsText.text = "";
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

    [ServerRpc(RequireOwnership = false)]
    public void UpdateLeaderboardServerRpc()
    {
        UpdateLeaderboardClientRpc();
    }

    [ClientRpc]
    public void UpdateLeaderboardClientRpc()
    {
        foreach (GameObject spawnedText in spawnedLeaderboardTexts)
        {
            Destroy(spawnedText);
        }
        spawnedLeaderboardTexts.Clear();

        if (leaderBoardPanel != null)
        {
            leaderBoardPanel.SetActive(true);
            foreach (var key in CheckpointManager.instance.GetLeaderboard().Keys)
            {
                GameObject _leaderBoardText = Instantiate(leaderBoardTextPrefab, leaderBoardPanel.transform);
                if (_leaderBoardText.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI text))
                {
                    text.text = $"{key.playerName}: {CheckpointManager.instance.GetLeaderboard()[key]}";
                    spawnedLeaderboardTexts.Add(_leaderBoardText);
                }
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

    public void SetPlayerCharacter(Character character)
    {
        playerCharacter = character;
    }

    public void UpdateFinalCharacterScore()
    {
        playerCharacter.SetPlayerScore(totalPoints);
    }
}












//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using Unity.Netcode;


//public class PlayerBombTrackerPointSystem : NetworkBehaviour
//{
//	public TextMeshProUGUI pointsText; // Assign this in the inspector
//	public TextMeshProUGUI livePointsText;
//	public int totalPoints { get; private set; }
//	[SerializeField] private int pointsPerSecond = 5;
//	private float bombHoldTime = 0f;
//	private bool hasBomb = false;
//	private float timeToAddPoints = 1f;
//	private float previousPointsTime;
//	public TextMeshProUGUI currentLapText;
//	public GameObject leaderBoardTextPrefab;
//	[SerializeField]
//	private GameObject leaderboardTextParentPrefab;
//	private string playerName;
//	[SerializeField] private GameObject leaderBoardPanel;
//	[SerializeField] private List<GameObject> leaderBoardTextParents = new List<GameObject>();
//	private List<GameObject> spawnedLeaderboardTexts = new List<GameObject>();

//	public Character playerCharacter { get; private set; }

//	[SerializeField]
//	private GameObject bombOwnerFeedback;

//	private void Awake()
//	{
//		totalPoints = 0;

//		if (leaderBoardPanel != null)
//			leaderBoardPanel.SetActive(false);

//	}

//	public override void OnNetworkSpawn()
//	{
//		base.OnNetworkSpawn();
//		AddToTotalPoints(1);
//	}
//	private void Update()
//	{
//		if (hasBomb)
//		{
//			if (previousPointsTime < timeToAddPoints)
//			{
//				previousPointsTime += Time.deltaTime;
//			}

//			else
//			{
//				previousPointsTime = 0;
//				AddToTotalPoints(pointsPerSecond);
//				UpdateUI();
//			}

//			bombOwnerFeedback.SetActive(true);
//		}
//		else
//		{
//			bombOwnerFeedback.SetActive(false);
//		}
//	}

//	public void PickUpBomb()
//	{
//		hasBomb = true;
//	}

//	public void DropBomb()
//	{
//		hasBomb = false;
//		bombOwnerFeedback?.SetActive(false);
//	}

//	public int GetTotalPoints()
//	{
//		return totalPoints;
//	}

//	private void UpdateUI()
//	{
//		if (pointsText != null)
//		{
//			pointsText.text = totalPoints.ToString();
//		}

//		if (currentLapText != null)
//		{
//			if (TryGetComponent<JankyCarControl_Multiplayer>(out JankyCarControl_Multiplayer carScript))
//			{
//				int maxLapCount = CheckpointManager.instance.totalLaps;
//				int currentLap = carScript.GetFinishedLapCount();
//				currentLapText.text = $"{currentLap}/ {maxLapCount}";


//			}

//		}
//	}

//	public void AddToTotalPoints(int newPoints)
//	{
//		totalPoints += newPoints;
//		UpdateUI();
//		UpdateLivePoints(newPoints);
//	}

//	public void UpdateLeaderboardUI()
//	{
//		foreach (GameObject spawnedText in spawnedLeaderboardTexts)
//		{
//			Destroy(spawnedText);
//		}
//		spawnedLeaderboardTexts.Clear();

//		if (leaderBoardPanel != null)
//		{
//		leaderBoardPanel.SetActive(true);
//		foreach (var key in CheckpointManager.instance.GetLeaderboard().Keys)
//		{
//			GameObject _leaderBoardText = Instantiate(leaderBoardTextPrefab, leaderBoardPanel.transform);
//			if (_leaderBoardText.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI text))
//			{
//				text.text = $"{key.playerName}: {CheckpointManager.instance.GetLeaderboard()[key]}";
//				spawnedLeaderboardTexts.Add(_leaderBoardText);
//			}
//		}
//		}

//	}

//	public void SetPlayerString(string _playerName)
//	{
//		playerName = _playerName;
//	}

//	public string GetPlayerString()
//	{
//		return playerName;
//	}

//	public void UpdateLivePoints(int _livePoints)
//	{
//		if (livePointsText != null)
//		{
//			if (_livePoints >= 1000) // Large lap bonus
//			{
//				StartCoroutine(ShowLapBonus(_livePoints));
//			}
//			else
//			{
//				StartCoroutine(ShowLivePointIncrements(_livePoints));
//			}
//		}
//	}

//	private IEnumerator ShowLivePointIncrements(int pointsToAdd)
//	{
//			livePointsText.text = $"+{pointsToAdd}"; // Show "+1" for each small increment
//			yield return new WaitForSeconds(0.2f); // Small delay between each "+1"
//		    livePointsText.text = ""; // Clear the text after all increments
//	}

//	private IEnumerator ShowLapBonus(int lapBonus)
//	{
//		livePointsText.text = $"+{lapBonus}"; // Show full lap bonus
//		yield return new WaitForSeconds(1.5f); // Keep it visible for 1.5 sec
//		livePointsText.text = ""; // Clear it after delay
//	}

//	public void SetPlayerCharacter(Character character)
//	{
//		playerCharacter = character;
//	}

//	public void UpdateFinalCharacterScore()
//	{
//		playerCharacter.SetPlayerScore(totalPoints);
//	}
//}
