using Unity.Netcode;
using UnityEngine;

public class SpawnManager : NetworkBehaviour
{
    public static SpawnManager Instance;
    public Transform[] spawnPoints; // Assign in the Inspector

    private void Awake()
    {
        Instance = this;
    }

    public void SpawnPlayers()
    {
        if (!IsServer) return;

        int index = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Transform spawnPoint = spawnPoints[index % spawnPoints.Length];
            GameObject playerInstance = Instantiate(Resources.Load<GameObject>("PlayerPrefab"), spawnPoint.position, spawnPoint.rotation);
            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId);

            index++;
        }
    }
}