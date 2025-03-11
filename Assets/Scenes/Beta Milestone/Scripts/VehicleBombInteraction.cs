using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class VehicleBombInteraction : NetworkBehaviour
{
    [Header("Bomb Interaction")]
    [SerializeField] private KeyCode throwBombKey = KeyCode.T; // Key to throw the bomb

    // Reference to the bomb currently held, if any.
    private BombBehaviour bombHeld;
    private PlayerBombTrackerPointSystem bombTracker; // Reference to the point tracking script

    private void Start()
    {
        // Assume the PlayerBombTracker is attached to the same GameObject
        bombTracker = GetComponent<PlayerBombTrackerPointSystem>();
    }


    // Detect bomb pickup via trigger


    private void OnCollisionEnter(Collision collision)
    {
        // Ensure we don’t already have a bomb
        if (bombHeld == null && collision.gameObject.CompareTag("Bomb"))
        {
            BombBehaviour bomb = collision.gameObject.GetComponent<BombBehaviour>();
            if (bomb != null)
            {
                bombHeld = bomb;
                bombHeld.PickUp(gameObject);
                Debug.Log("Bomb picked up!");

                // Notify the point system
                bombTracker?.PickUpBombServerRpc();
            }
        }
    }




    //private void OnTriggerEnter(Collider other)
    //{
    //	// Ensure we don’t already have a bomb
    //	if (bombHeld == null && other.CompareTag("Bomb"))
    //	{
    //		BombBehaviour bomb = other.GetComponent<BombBehaviour>();
    //		if (bomb != null)
    //		{
    //			bombHeld = bomb;
    //			bombHeld.PickUp(gameObject);
    //			Debug.Log("Bomb picked up!");

    //			// Notify the point system
    //			bombTracker?.PickUpBomb();
    //		}
    //	}
    //}
    private void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.T) || IsOwner &&  Input.GetButtonDown("ControllerThrow")) // Ensure only the local player can trigger
        {
            RequestBombServerRPC();
            
        }

      
    }

    [ServerRpc]
    private void RequestBombServerRPC(ServerRpcParams rpcParams = default)
    {
        // Ensure only the player who requested the respawn gets moved
        BombClientRpc(rpcParams.Receive.SenderClientId);
    }


    [ClientRpc]
    private void BombClientRpc(ulong clientId)
    {


        if (OwnerClientId == clientId) // Only move the player who requested respawn
        { 
            Debug.Log("Bomb getting called");
            // Listen for the key to throw the bomb.
            if (bombHeld != null)
            {
                bombHeld.Throw();
                bombHeld = null;
                Debug.Log("Bomb thrown!}" );

                // Notify the point system that we've dropped/thrown the bomb.
                if (bombTracker != null)
                {
                    bombTracker.DropBombServerRpc();
                }
            }
        }
    }
}
