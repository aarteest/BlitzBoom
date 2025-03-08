using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class RaceManager : NetworkBehaviour
{
    public static RaceManager Singleton;
    private bool raceCompleted = false;

    private void Awake()
    {
        Singleton = this;
    }

    public void EndRace()
    {
        if (!IsServer || raceCompleted) return;

        raceCompleted = true; // Ensure it triggers only once
        
        StartCoroutine(TransitionToNewScene());
    }

    private IEnumerator TransitionToNewScene()
    {
        yield return new WaitForSeconds(3f);
        NetworkManager.Singleton.SceneManager.LoadScene("WinScene", UnityEngine.SceneManagement.LoadSceneMode.Single);

        NetworkManager.Singleton.SceneManager.OnLoadComplete += (ulong clientId, string sceneName, UnityEngine.SceneManagement.LoadSceneMode mode) =>
        {
            if (sceneName == "WinScene" && IsServer)
            {
                SpawnManager.Instance.SpawnPlayers();
            }
        };
    }
}