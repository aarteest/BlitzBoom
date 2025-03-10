using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.IO;
using UnityEngine.Events;

public class BombManager : NetworkBehaviour
{

    [SerializeField]
    private List<GameObject> allPlayers = new List<GameObject>();

    public GameObject bombPrefab;
    [SerializeField] private GameObject loadedPrefab;

    public GameObject spawnedBomb;

    public TextMeshProUGUI bombText;

    string path;

    Vector3 pos = Vector3.zero;
    private void Start()
    {
        bombText.text = null;

        CheckBomb();
    }

    private void OnEnable()
    {

    }

    private void OnDisable()
    {

    }

    public override void OnNetworkSpawn()
    {

        path = Application.persistentDataPath + "/log.txt";

        StartCoroutine(AddNetworkSpawnDelay());
    }

    private void ChoosePlayers()
    {
        if (IsServer)
        {
            if (allPlayers.Count > 0)
            {
                int randomIndex = Random.Range(0, allPlayers.Count);

                GameObject chosenPlayer = allPlayers[randomIndex];

                Transform bombHolder = chosenPlayer.transform.Find("BombHolder");

                StartCoroutine(MoveBomb(bombHolder));
            }

        }
    }

    public void SpawnBombOnRandomPlayer()
    {
        if (allPlayers.Count == 0) return;

        int randomIndex = Random.Range(0, allPlayers.Count);

        GameObject chosenPlayer = allPlayers[randomIndex];

        Transform bombHolder = chosenPlayer.transform.Find("BombHolder");

        StartCoroutine(MoveBomb(bombHolder));
    }

    public void SpawnBomb()
    {
        if (IsServer)
        {
            if (bombPrefab != null)
            {
                loadedPrefab = bombPrefab;
                Debug.Log("Using Inspector-assigned bombPrefab.");
            }
            else
            {
                loadedPrefab = Resources.Load<GameObject>("Bomb_Prefab");
                Debug.Log(loadedPrefab == null ? "Failed to load Bomb_Prefab from Resources!" : "Successfully loaded Bomb_Prefab.");
            }

            spawnedBomb = Instantiate(loadedPrefab, Vector3.zero, Quaternion.identity);
            spawnedBomb.GetComponent<NetworkObject>().Spawn(); // Spawn for all clients
            //spawnedBomb.GetComponent<Rigidbody>().isKinematic = true;
            spawnedBomb.GetComponent<BombBehaviour>().enabled = false;

            if (spawnedBomb != null)
            {
                ChoosePlayers();
            }
        }
    }

    IEnumerator MoveBomb(Transform chosenHolder)
    {
        File.AppendAllText(path, "Bomb shoud start countdown" + "\n");

        int countdownTime = 3;

        while (countdownTime > 0)
        {
            UpdateUITextServerRPC("Choosing Player and Spawning Bomb..." + countdownTime);
            File.AppendAllText(path, bombText.text + "\n");

            yield return new WaitForSeconds(1f);

            countdownTime--;
        }

        if (countdownTime <= 0)
        {
            UpdateUITextServerRPC(null);
            spawnedBomb.transform.position = chosenHolder.transform.position;
            spawnedBomb.GetComponent<Rigidbody>().isKinematic = false;
            spawnedBomb.GetComponent<BombBehaviour>().enabled = true;
            UpdateBombPostClientRPC(spawnedBomb.transform.position);
        }
    }

    void CheckBomb()
    {
        string message = spawnedBomb != null ? spawnedBomb.name + "  " + spawnedBomb.transform.position.ToString() : "Object Nakko Spawned";
        File.AppendAllText(path, message + "\n");

    }

    [ServerRpc]
    void UpdateUITextServerRPC(string message)
    {
        UpdateUITextClientRPC(message);
    }

    [ClientRpc]
    void UpdateUITextClientRPC(string message)
    {
        bombText.text = message;
    }

    [ClientRpc]
    void UpdateBombPostClientRPC(Vector3 newPosition)
    {
        if (IsOwner || spawnedBomb == null) return;  // Ensure we don't update null objects
        spawnedBomb.transform.position = newPosition;
    }

    void PopulateList()
    {
        if (IsServer)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player"); // Ensure all vehicles are tagged properly

            // Only add players that have a BombHolder
            foreach (GameObject player in players)
            {
                if (player.transform.Find("BombHolder") != null)
                {
                    allPlayers.Add(player);
                }
            }

            if (allPlayers.Count > 0)
            {
                SpawnBomb();
            }
        }

    }

    IEnumerator AddNetworkSpawnDelay()
    {
        yield return new WaitForSeconds(1f);
        PopulateList();
    }
        
}


