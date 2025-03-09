using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class VehicleRespawnCollider : NetworkBehaviour
{
    private Transform lastCheckpoint; // Last checkpoint the vehicle passed
    [SerializeField] private float respawnYOffset = 1f; // Small offset to prevent clipping
    
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (lastCheckpoint == null)
        {
            Debug.LogWarning("No checkpoint assigned! Assigning vehicle's starting position.");
            lastCheckpoint = new GameObject("DefaultCheckpoint").transform;
            lastCheckpoint.position = transform.position;
            lastCheckpoint.rotation = transform.rotation;
        }
    }

    private void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.R)) // Ensure only the local player can trigger
        {
            RequestRespawnServerRpc();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Checkpoint"))
        {
            lastCheckpoint = other.transform;
            //Debug.Log("Checkpoint Updated: " + lastCheckpoint.position);
        }
        
    }

    [ServerRpc]
    private void RequestRespawnServerRpc(ServerRpcParams rpcParams = default)
    {
        // Ensure only the player who requested the respawn gets moved
        RespawnClientRpc(rpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void RespawnClientRpc(ulong clientId)
    {
        if (OwnerClientId == clientId) // Only move the player who requested respawn
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = lastCheckpoint.position + Vector3.up * respawnYOffset;
            transform.rotation = lastCheckpoint.rotation;
            Debug.Log($"Player {clientId} respawned at checkpoint.");
        }
    }
}
















//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Unity.Netcode;

//public class VehicleRespawnCollider : NetworkBehaviour
//{
//	private Transform lastCheckpoint; // Last checkpoint the vehicle passed
//	[SerializeField] private float respawnYOffset = 1f; // Small offset to prevent clipping

//	private void Start()
//	{
//		if (lastCheckpoint == null)
//		{
//			Debug.LogWarning("No checkpoint assigned! Assigning vehicle's starting position.");
//			lastCheckpoint = new GameObject("DefaultCheckpoint").transform;
//			lastCheckpoint.position = transform.position;
//			lastCheckpoint.rotation = transform.rotation;
//		}
//	}

//	private void Update()
//	{
//		if (Input.GetKeyDown(KeyCode.R))
//		{

//            Respawn();
//		}
//	}


//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Checkpoint"))
//        {
//            lastCheckpoint = other.GetComponent<Transform>();
//            Debug.Log(lastCheckpoint.transform.position);
//        }
//    }

//    private void Respawn()
//    {
//        //transform.Translate(lastCheckpoint.transform.position);
//        transform.position = lastCheckpoint.position;
//    }

//    ////////////////////////////////////////////////////////////////////////////////
//    //private void OnTriggerEnter(Collider other)
//    //{
//    //	if (other.CompareTag("Checkpoint"))
//    //	{
//    //		lastCheckpoint = other.transform;
//    //		Debug.Log("Checkpoint Passed: " + lastCheckpoint.name);
//    //		Debug.Log(lastCheckpoint.transform.po);
//    //	}
//    //	else if (other.CompareTag("FallZone"))
//    //	{
//    //		Debug.Log("Fell off track! Respawning...");
//    //		Respawn();
//    //	}
//    //}

//    //private void Respawn()
//    //{
//    //	if (!IsOwner) return;

//    //	if (lastCheckpoint != null)
//    //	{
//    //		//Debug.Log(lastCheckpoint);
//    //		VehicleDebuff vDebuff = GetComponent<VehicleDebuff>();
//    //		vDebuff.ResetCar();

//    //		transform.position = lastCheckpoint.position + Vector3.up * respawnYOffset;
//    //		transform.rotation = lastCheckpoint.rotation;
//    //		Debug.Log("Respawned at last checkpoint: " + lastCheckpoint.name);
//    //	}
//    //	else
//    //	{
//    //		Debug.LogError("No valid checkpoint found! Vehicle is stuck.");
//    //	}
//    //}
//}
