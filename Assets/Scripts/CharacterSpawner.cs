using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class CharacterSpawner : NetworkBehaviour
{

    [Header("References")]
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private List<Transform> spawnPoints; // Serialized list of spawn points

    public override void OnNetworkSpawn()
    {

        //Debug.Log("Reached Server Line");

        // Ensure the code runs only on the server
        if (!IsHost) return;
        Debug.Log("Passed server line");

        // Make a copy of the spawn points to avoid modifying the original list
        var availableSpawnPoints = new List<Transform>(spawnPoints);

        Debug.Log(HostManager.Instance.ClientData);

        foreach (var client in HostManager.Instance.ClientData)
        {
            Debug.Log("Entered foreach loop");

            // Retrieve the character using its ID
            var character = characterDatabase.GetCharacterById(client.Value.characterId);
            if (character != null && availableSpawnPoints.Count > 0)
            {
                // Select and remove a spawn point from the list
                int spawnIndex = Random.Range(0, availableSpawnPoints.Count);
                Transform selectedSpawnPoint = availableSpawnPoints[spawnIndex];
                availableSpawnPoints.RemoveAt(spawnIndex); // Remove the used spawn point

                // Spawn the character at the selected spawn point
                var characterInstance = Instantiate(character.GameplayPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);
                characterInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.Value.clientId);
                if (characterInstance.TryGetComponent<PlayerBombTrackerPointSystem>(out PlayerBombTrackerPointSystem playerScript))
                {
                    playerScript.SetPlayerString(character.DisplayName);
                    playerScript.SetPlayerCharacter(character);
                }
                //characterInstance.SpawnAsPlayerObject(client.Value.clientId);
                Debug.Log($"Character ownership assigned to Client ID: {characterInstance.OwnerClientId}");
                DebugClientData();

                Debug.Log("Player spawned at unique point");
            }
            else
            {
                Debug.LogWarning("No available spawn points or invalid character!");
            }
        }


    }

    public void DebugClientData()
    {
        Debug.Log("Current Client Data:");
        foreach (var client in HostManager.Instance.ClientData)
        {
            Debug.Log($"Client ID: {client.Key}, Character ID: {client.Value.characterId}");
        }
    }



    //[Header("References")]
    //[SerializeField] private CharacterDatabase characterDatabase;

    //public override void OnNetworkSpawn()
    //{
    //    Debug.Log("Reached Server Line");

    //    //if (!IsServer) { return; }
    //    Debug.Log("Passed server line");


    //    foreach (var client in HostManager.Instance.ClientData)
    //    {
    //        Debug.Log("enterred for each");

    //        var character = characterDatabase.GetCharacterById(client.Value.characterId);
    //        if (character != null)
    //        {
    //            var spawnPos = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
    //            var characterInstance = Instantiate(character.GameplayPrefab, spawnPos, Quaternion.identity);
    //            characterInstance.SpawnAsPlayerObject(client.Value.clientId);
    //            Debug.Log("Player C");
    //        }
    //    }
    //}
}
