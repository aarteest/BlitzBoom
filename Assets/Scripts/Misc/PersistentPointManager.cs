using Unity.Netcode;
using System.Collections.Generic;

public class PersistentPointManager : NetworkBehaviour
{
    // Singleton for easy access
    public static PersistentPointManager Instance { get; private set; }

    // Dictionary to hold points for each player
    private Dictionary<ulong, int> playerPoints = new Dictionary<ulong, int>();

    private void Awake()
    {
        // Set the singleton instance
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // ServerRpc to add points
    [ServerRpc]
    public void AddPointsServerRpc(ulong clientId, int amount)
    {
        if (!playerPoints.ContainsKey(clientId))
        {
            playerPoints[clientId] = 0;
        }
        playerPoints[clientId] += amount;

        // Optionally, save this data persistently (e.g., in a file or database)
    }

    // ServerRpc to get points for a specific player
    public int GetPlayerPoints(ulong clientId)
    {
        if (playerPoints.ContainsKey(clientId))
        {
            return playerPoints[clientId];
        }
        return 0;
    }
}

















//using System.Collections.Generic;
//using UnityEngine;
//using Unity.Netcode;

//public class PersistentPointManager : NetworkBehaviour
//{
//    public static PersistentPointManager Instance { get; private set; }

//    private Dictionary<ulong, int> playerPoints = new Dictionary<ulong, int>();

//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }
//        Instance = this;
//        DontDestroyOnLoad(gameObject);
//    }

//    public override void OnNetworkSpawn()
//    {
//        if (IsServer)
//        {
//            playerPoints.Clear(); // Reset points when game starts
//        }
//    }

//    // Increase points for a specific player
//    [ServerRpc(RequireOwnership = false)]
//    public void AddPointsServerRpc(ulong playerId, int points)
//    {
//        if (!playerPoints.ContainsKey(playerId))
//            playerPoints[playerId] = 0;

//        playerPoints[playerId] += points;
//        UpdateClientPointsClientRpc(playerId, playerPoints[playerId]);
//    }

//    // Sync points to clients
//    [ClientRpc]
//    private void UpdateClientPointsClientRpc(ulong playerId, int newPoints)
//    {
//        if (NetworkManager.Singleton.LocalClientId == playerId)
//        {
//            PointSystem.LocalInstance?.UpdatePointsUI(newPoints);
//        }
//    }

//    // Get stored points (useful for displaying at race start)
//    public int GetPlayerPoints(ulong playerId)
//    {
//        return playerPoints.ContainsKey(playerId) ? playerPoints[playerId] : 0;
//    }
//}