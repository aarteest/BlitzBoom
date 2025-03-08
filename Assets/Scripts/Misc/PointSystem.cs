using UnityEngine;
using TMPro;
using Unity.Netcode;

public class PointSystem : NetworkBehaviour
{
    public static PointSystem LocalInstance { get; private set; }

    public TMP_Text pointsText;
    private NetworkVariable<int> currentPoints = new NetworkVariable<int>(0); // Using NetworkVariable to sync points

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            LocalInstance = this;

            // Ensure pointsText is assigned correctly on all clients
            if (pointsText == null)
            {
                GameObject pointsObject = GameObject.FindGameObjectWithTag("Points");
                if (pointsObject != null)
                {
                    pointsText = pointsObject.GetComponent<TMP_Text>();
                }
                else
                {
                    Debug.LogError("No GameObject with tag 'Points' found!");
                }
            }

            // Load previous points from Persistent Manager
            currentPoints.Value = PersistentPointManager.Instance.GetPlayerPoints(NetworkManager.Singleton.LocalClientId);
            UpdatePointsUI(currentPoints.Value);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (IsOwner && collision.gameObject.CompareTag("Boss"))
        {
            AddPoints(50); // Adds points when colliding with Boss
        }
    }

    // Method to add points
    public void AddPoints(int amount)
    {
        if (IsOwner)
        {
            currentPoints.Value += amount; // Update points locally

            // Sync points with server
            AddPointsServerRpc(NetworkManager.Singleton.LocalClientId, amount);
        }
    }

    // ServerRPC to update points on the server and sync with clients
    [ServerRpc]
    void AddPointsServerRpc(ulong clientId, int amount)
    {
        // Update the player's points on the server
        PersistentPointManager.Instance.AddPointsServerRpc(clientId, amount);

        // Update points for all clients
        UpdatePointsClientRpc(currentPoints.Value);
    }

    // ClientRPC to update the points for all clients
    [ClientRpc]
    void UpdatePointsClientRpc(int newPoints)
    {
        currentPoints.Value = newPoints;
        UpdatePointsUI(newPoints);
    }

    // Method to update the UI
    public void UpdatePointsUI(int newPoints)
    {
        if (pointsText != null)
            pointsText.text = "Points: " + newPoints;
    }
}











//using UnityEngine;
//using TMPro;
//using Unity.Netcode;

//public class PointSystem : NetworkBehaviour
//{
//    public static PointSystem LocalInstance { get; private set; }

//    public TMP_Text pointsText;
//    private int currentPoints = 0;

//    public override void OnNetworkSpawn()
//    {
//        if (IsOwner)
//        {
//            LocalInstance = this;

//            if (pointsText == null)
//            {
//                GameObject pointsObject = GameObject.FindGameObjectWithTag("Points");
//                if (pointsObject != null)
//                {
//                    pointsText = pointsObject.GetComponent<TMP_Text>();
//                }
//                else
//                {
//                    Debug.LogError("No GameObject with tag 'Points' found!");
//                }
//            }

//            // Load previous points from Persistent Manager
//            currentPoints = PersistentPointManager.Instance.GetPlayerPoints(NetworkManager.Singleton.LocalClientId);
//            UpdatePointsUI(currentPoints);
//        }
//    }

//    void OnCollisionEnter(Collision collision)
//    {
//        if (IsOwner && collision.gameObject.CompareTag("Boss"))
//        {
//            AddPoints(50);
//        }
//    }

//    public void AddPoints(int amount)
//    {
//        if (IsOwner)
//        {
//            PersistentPointManager.Instance.AddPointsServerRpc(NetworkManager.Singleton.LocalClientId, amount);
//        }
//    }

//    public void UpdatePointsUI(int newPoints)
//    {
//        currentPoints = newPoints;
//        if (pointsText != null)
//            pointsText.text = "Points: " + currentPoints;
//    }
//}

















//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using Unity.Netcode;

//public class PointSystem : NetworkBehaviour
//{
//    private NetworkVariable<int> playerPoints = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

//    public TMP_Text pointsText;

//    private float slipstreamTime = 0f;
//    private bool isInSlipstream = false;

//    public override void OnNetworkSpawn()
//    {
//        if (IsOwner)
//        {
//            if (pointsText == null)
//            {
//                GameObject pointsObject = GameObject.FindGameObjectWithTag("Points");
//                if (pointsObject != null)
//                {
//                    pointsText = pointsObject.GetComponent<TMP_Text>();
//                }
//                else
//                {
//                    Debug.LogError("No GameObject with tag 'Points' found in the scene!");
//                }
//            }
//            UpdatePointsUI(playerPoints.Value, playerPoints.Value);
//        }

//        // Subscribe to point updates for all clients
//        playerPoints.OnValueChanged += UpdatePointsUI;
//    }

//    public override void OnDestroy()
//    {
//        base.OnDestroy();
//        playerPoints.OnValueChanged -= UpdatePointsUI;
//    }

//    // Collision detection for hitting a boss
//    void OnCollisionEnter(Collision collision)
//    {
//        if (IsOwner && collision.gameObject.CompareTag("Boss"))
//        {
//            OnHitBossServerRpc();
//        }
//    }

//    // Server RPC for adding points when hitting a boss
//    [ServerRpc]
//    public void OnHitBossServerRpc()
//    {
//        playerPoints.Value += 50;
//    }

//    // Server RPC for adding points when dodging a boss ability
//    [ServerRpc]
//    public void OnAvoidBossAbilityServerRpc()
//    {
//        playerPoints.Value += 30;
//    }

//    // Server RPC for adding points when overtaking another player
//    [ServerRpc]
//    public void OnOvertakePlayerServerRpc()
//    {
//        playerPoints.Value += 40;
//    }

//    // Server RPC for adding points when dodging an obstacle
//    [ServerRpc]
//    public void OnDodgeObstacleServerRpc()
//    {
//        playerPoints.Value += 20;
//    }

//    // Server RPC for adding points when performing a near miss
//    [ServerRpc]
//    public void OnNearMissServerRpc()
//    {
//        playerPoints.Value += 10;
//        Debug.Log("Near Miss Triggered! Points: " + playerPoints.Value);
//    }

//    // Client calls this when entering slipstream
//    public void EnterSlipstream()
//    {
//        if (IsOwner)
//        {
//            isInSlipstream = true;
//            slipstreamTime = 0f;
//        }
//    }

//    // Client calls this when exiting slipstream
//    public void ExitSlipstream()
//    {
//        if (IsOwner && isInSlipstream && slipstreamTime >= 3f)
//        {
//            OnExitSlipstreamServerRpc();
//        }
//        isInSlipstream = false;
//    }

//    [ServerRpc]
//    public void OnExitSlipstreamServerRpc()
//    {
//        playerPoints.Value += 25;
//    }

//    void Update()
//    {
//        if (IsOwner && isInSlipstream)
//        {
//            slipstreamTime += Time.deltaTime;
//        }
//    }

//    // Updates the UI for all players
//    private void UpdatePointsUI(int oldPoints, int newPoints)
//    {
//        if (pointsText != null)
//            pointsText.text = "Points: " + newPoints;
//    }
//}






//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//public class PointSystem : MonoBehaviour
//{
//	private int playerPoints = 0;
//	public TMP_Text pointsText;

//	private float slipstreamTime = 0f;
//	private bool isInSlipstream = false;


//    void Start()
//    {
//        if (pointsText == null)
//        {
//            GameObject pointsObject = GameObject.FindGameObjectWithTag("Points");
//            if (pointsObject != null)
//            {
//                pointsText = pointsObject.GetComponent<TMP_Text>();
//            }
//            else
//            {
//                Debug.LogError("No GameObject with tag 'points' found in the scene!");
//            }
//        }

//        UpdatePointsUI();
//    }


//    //void Start()
//    //{


//    //	UpdatePointsUI();
//    //}

//    // Collision detection inside PlayerPoints itself
//    void OnCollisionEnter(Collision collision)
//	{
//		if (collision.gameObject.CompareTag("Boss"))
//		{
//			OnHitBoss();
//		}
//	}

//	// Call this when player hits the boss in OnCollisionEnter
//	public void OnHitBoss()
//	{
//		playerPoints += 50;
//		UpdatePointsUI();
//	}

//	// Call this when player dodges a boss ability
//	public void OnAvoidBossAbility()
//	{
//		playerPoints += 30;
//		UpdatePointsUI();
//	}

//	// Call this when player overtakes another player
//	public void OnOvertakePlayer()
//	{
//		playerPoints += 40;
//		UpdatePointsUI();
//	}

//	// Call this when player dodges an obstacle
//	public void OnDodgeObstacle()
//	{
//		playerPoints += 20;
//		UpdatePointsUI();
//	}

//	// Call this when player gets a near miss (within 1.5m of obstacle/player)
//	public void OnNearMiss()
//	{
//		playerPoints += 10;
//		Debug.Log("Near Miss Triggered! Points: " + playerPoints);
//		UpdatePointsUI();
//	}

//	// Call this when player enters another racer's slipstream
//	public void EnterSlipstream()
//	{
//		isInSlipstream = true;
//		slipstreamTime = 0f;
//	}

//	// Call this when player exits slipstream
//	public void ExitSlipstream()
//	{
//		if (isInSlipstream && slipstreamTime >= 3f)
//		{
//			playerPoints += 25;
//			UpdatePointsUI();
//		}
//		isInSlipstream = false;
//	}

//	void Update()
//	{
//		// Track slipstream time
//		if (isInSlipstream)
//		{
//			slipstreamTime += Time.deltaTime;
//		}
//	}

//	void UpdatePointsUI()
//	{
//		if (pointsText != null)
//			pointsText.text = "Points: " + playerPoints;
//	}
//}
