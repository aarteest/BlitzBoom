using UnityEngine;
using Unity.Netcode;

public class CheckpointRespawn : NetworkBehaviour
{
    public static CheckpointRespawn instance;

    private Vector3 lastCheckpointPosition; // Stores the last checkpoint position
    private Quaternion lastCheckpointRotation; // Stores the last checkpoint rotation
    private string lastCheckpointName = "None"; // Stores the last checkpoint name

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (IsOwner)
        {
            // Set the initial respawn position to the player's starting position
            lastCheckpointPosition = transform.position;
            lastCheckpointRotation = transform.rotation;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Check if the player presses Backspace to respawn
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Debug.Log("Respawning at: " + lastCheckpointName);
            RequestRespawnServerRpc();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        // Check if the object is a checkpoint
        if (other.CompareTag("Checkpoint"))
        {
            UpdateCheckpointServerRpc(other.transform.position, other.transform.rotation, other.gameObject.name);
        }
    }

    [ServerRpc]
    private void UpdateCheckpointServerRpc(Vector3 position, Quaternion rotation, string checkpointName)
    {
        lastCheckpointPosition = position;
        lastCheckpointRotation = rotation;
        lastCheckpointName = checkpointName;
        Debug.Log("Checkpoint Triggered: " + lastCheckpointName);
    }

    [ServerRpc]
    public void RequestRespawnServerRpc()
    {
        RespawnPlayerClientRpc();
    }

    [ClientRpc]
    private void RespawnPlayerClientRpc()
    {
        transform.position = lastCheckpointPosition;
        transform.rotation = lastCheckpointRotation;
        rb.velocity = Vector3.zero; // Stop movement
        rb.angularVelocity = Vector3.zero; // Stop rotation
    }
}






//using UnityEngine;

//public class CheckpointRespawn : MonoBehaviour
//{
//	private Vector3 lastCheckpointPosition; // Stores the last checkpoint position
//	private Quaternion lastCheckpointRotation; // Stores the last checkpoint rotation
//	private string lastCheckpointName = "None"; // Stores the name of the last checkpoint
//	private void Start()
//	{
//		// Set the initial respawn position to the vehicle's starting position
//		lastCheckpointPosition = transform.position;
//		lastCheckpointRotation = transform.rotation;
//	}

//	private void Update()
//	{
//		// Check if the player presses Backspace to respawn
//		if (Input.GetKeyDown(KeyCode.Backspace))
//		{
//			Debug.Log("Respawning at: " + lastCheckpointName);
//			RespawnAtLastCheckpoint();
//		}
//	}

//	private void OnTriggerEnter(Collider other)
//	{
//		// Check if the object is a checkpoint
//		if (other.CompareTag("Checkpoint"))
//		{
//			lastCheckpointPosition = other.transform.position;
//			lastCheckpointRotation = other.transform.rotation;
//			lastCheckpointName = other.gameObject.name; // Get the name from the Hierarchy

//			Debug.Log("Checkpoint Triggered: " + lastCheckpointName);
//		}
//	}

//	private void RespawnAtLastCheckpoint()
//	{
//		// Move vehicle to the last checkpoint
//		transform.position = lastCheckpointPosition;
//		transform.rotation = lastCheckpointRotation;
//		GetComponent<Rigidbody>().velocity = Vector3.zero; // Stop movement
//		GetComponent<Rigidbody>().angularVelocity = Vector3.zero; // Stop rotation
//	}
//}
