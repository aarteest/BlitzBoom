using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class PlayerBombTrackerPointSystem : NetworkBehaviour
{
    private TextMeshProUGUI pointsText;
    private TextMeshProUGUI livePointsText;

    public NetworkVariable<int> totalPoints = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> hasBombNetwork = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [SerializeField] private int pointsPerSecond = 5;
    private float previousPointsTime;
    private float timeToAddPoints = 1f;

    public TextMeshProUGUI currentLapText;
    public GameObject leaderBoardTextPrefab;
    [SerializeField] private GameObject leaderboardTextParentPrefab;
    private string playerName;
    [SerializeField] private GameObject leaderBoardPanel;
    private List<GameObject> spawnedLeaderboardTexts = new List<GameObject>();

    public Character playerCharacter { get; private set; }
    [SerializeField] private GameObject bombOwnerFeedback;

    private void Awake()
    {
        if (leaderBoardPanel != null)
            leaderBoardPanel.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            totalPoints.Value = 0;
        }
        if (IsOwner)
        {
            pointsText = GameObject.Find("PointsText")?.GetComponent<TextMeshProUGUI>();
            livePointsText = GameObject.Find("LivePointsText")?.GetComponent<TextMeshProUGUI>();
        }
        UpdateUIClientRpc(totalPoints.Value);
    }

    private void Update()
    {
        if (hasBombNetwork.Value && IsOwner)
        {
            if (previousPointsTime < timeToAddPoints)
            {
                previousPointsTime += Time.deltaTime;
            }
            else
            {
                previousPointsTime = 0;
                SubmitPointsToServerRpc(pointsPerSecond);
            }
            bombOwnerFeedback.SetActive(true);
        }
        else
        {
            bombOwnerFeedback.SetActive(false);
        }
    }

    public void PickUpBomb()
    {
        if (IsOwner)
        {
            AttemptToPickUpBomb();
            Debug.Log("Bomb Attemped To Pick");
            PickUpBombServerRpc();
        }
    }

    public void DropBomb()
    {
        if (IsOwner)
        {
            DropBombServerRpc();
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void PickUpBombServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong senderClientId = rpcParams.Receive.SenderClientId;
        Debug.Log($"[SERVER] PickUpBombServerRpc called by Client ID: {senderClientId}");

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(senderClientId, out var client))
        {
            var playerTracker = client.PlayerObject.GetComponent<PlayerBombTrackerPointSystem>();
            if (playerTracker != null)
            {
                playerTracker.hasBombNetwork.Value = true;
                Debug.Log($"[SERVER] Bomb assigned to player {senderClientId}");
            }
            else
            {
                Debug.LogError($"[SERVER] PlayerBombTrackerPointSystem component not found on Client {senderClientId}");
            }
        }
        else
        {
            Debug.LogError($"[SERVER] Client ID {senderClientId} not found in ConnectedClients");
        }
    }


    //[ServerRpc(RequireOwnership = false)]
    //private void PickUpBombServerRpc()
    //{
    //    hasBombNetwork.Value = true;
    //    Debug.Log("Has Bomb");
    //}

    [ServerRpc(RequireOwnership = false)]
    private void DropBombServerRpc()
    {
        hasBombNetwork.Value = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitPointsToServerRpc(int pointsToAdd, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            var playerTracker = client.PlayerObject.GetComponent<PlayerBombTrackerPointSystem>();
            if (playerTracker != null && playerTracker.hasBombNetwork.Value)
            {
                playerTracker.AddToTotalPointsServerRpc(pointsToAdd, clientId);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddToTotalPointsServerRpc(int newPoints, ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            var playerTracker = client.PlayerObject.GetComponent<PlayerBombTrackerPointSystem>();
            if (playerTracker != null && playerTracker.hasBombNetwork.Value)
            {
                playerTracker.totalPoints.Value += newPoints;
                playerTracker.UpdateUIClientRpc(playerTracker.totalPoints.Value);
                playerTracker.UpdateLivePointsClientRpc(newPoints);
            }
        }
    }

    [ClientRpc]
    private void UpdateUIClientRpc(int updatedPoints)
    {

        if (IsOwner)
        {
            pointsText?.SetText(updatedPoints.ToString());
            if (currentLapText != null && TryGetComponent<JankyCarControl_Multiplayer>(out JankyCarControl_Multiplayer carScript))
            {
                int maxLapCount = CheckpointManager.instance.totalLaps;
                int currentLap = carScript.GetFinishedLapCount();
                currentLapText.text = $"{currentLap}/ {maxLapCount}";
            }
        }
    }

    [ClientRpc]
    private void UpdateLivePointsClientRpc(int _livePoints)
    {
        if (IsOwner && livePointsText != null)
        {
            StartCoroutine(_livePoints >= 1000 ? ShowLapBonus(_livePoints) : ShowLivePointIncrements(_livePoints));
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

    public void SetPlayerCharacter(Character character)
    {
        playerCharacter = character;
    }

    public void UpdateFinalCharacterScore()
    {
        playerCharacter.SetPlayerScore(totalPoints.Value);
    }

    public void UpdateLeaderboardUI()
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

    public void AttemptToPickUpBomb()
    {
        if (IsOwner)
        {
            Debug.Log($"[CLIENT {NetworkManager.Singleton.LocalClientId}] Attempting to pick up the bomb...");
            PickUpBombServerRpc();
        }
        else
        {
            Debug.Log("[CLIENT] Requesting ownership of the bomb...");
            GetComponent<NetworkObject>().ChangeOwnership(NetworkManager.Singleton.LocalClientId);
        }
    }

}













//ORIGINAL//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////






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
//			leaderBoardPanel.SetActive(true);
//			foreach (var key in CheckpointManager.instance.GetLeaderboard().Keys)
//			{
//				GameObject _leaderBoardText = Instantiate(leaderBoardTextPrefab, leaderBoardPanel.transform);
//				if (_leaderBoardText.TryGetComponent<TextMeshProUGUI>(out TextMeshProUGUI text))
//				{
//					text.text = $"{key.playerName}: {CheckpointManager.instance.GetLeaderboard()[key]}";
//					spawnedLeaderboardTexts.Add(_leaderBoardText);
//				}
//			}
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
//		livePointsText.text = $"+{pointsToAdd}"; // Show "+1" for each small increment
//		yield return new WaitForSeconds(0.2f); // Small delay between each "+1"
//		livePointsText.text = ""; // Clear the text after all increments
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
