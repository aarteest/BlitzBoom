using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class PlayerPoints : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI playerPointsText;  // UI element to display points

    // Network variable to store the player's points, synchronized across clients
    private NetworkVariable<int> playerPoints = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // Start is called before the first frame update
    void Start()
    {
        if (IsOwner)  // Ensure this script only runs for the local player (owner)
        {
            // Find the player's own points UI (TextMeshProUGUI) for the local player only
            playerPointsText = GameObject.FindGameObjectWithTag("playerPoints")?.GetComponent<TextMeshProUGUI>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner && playerPointsText != null)  // Ensure UI update happens for the local player only
        {
            playerPointsText.text = "Points: " + playerPoints.Value.ToString();  // Update UI text
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only allow the local player to add points (for multiplayer, should be done on the server)
        if (IsOwner && collision.collider.CompareTag("Boss"))
        {
            AddPointsServerRpc(50);  // Call the ServerRpc to add points
        }
    }

    // ServerRpc to add points on the server
    [ServerRpc]
    void AddPointsServerRpc(int pointsToAdd)
    {
        playerPoints.Value += pointsToAdd;  // Update points on the server
    }
}





//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using Unity.Netcode;

//public class PlayerPoints : NetworkBehaviour
//{
//    [SerializeField]
//    private TextMeshProUGUI playerPointsText;

//    // Network variable to store the player's points
//    private NetworkVariable<int> playerPoints = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

//    // Start is called before the first frame update
//    void Start()
//    {
//        if (IsOwner)  // Ensure this script only runs on the local player
//        {
//            playerPointsText = GameObject.FindGameObjectWithTag("playerPoints")?.GetComponent<TextMeshProUGUI>();
//        }
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        // Update the TextMeshProUGUI text with the player's current points
//        if (IsOwner && playerPointsText != null)
//        {
//            playerPointsText.text = "Points: " + playerPoints.Value.ToString();
//        }
//    }

//    private void OnCollisionEnter(Collision collision)
//    {
//        // Only update the score on the server
//        if (IsOwner && collision.collider.CompareTag("Boss"))
//        {
//            AddPointsServerRpc(50);  // Call the ServerRpc to add points
//        }
//    }

//    // ServerRpc to add points on the server
//    [ServerRpc]
//    void AddPointsServerRpc(int pointsToAdd)
//    {
//        playerPoints.Value += pointsToAdd;  // Update the points on the server
//    }
//}