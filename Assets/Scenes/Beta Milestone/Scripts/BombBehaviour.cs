using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;

public class BombBehaviour : NetworkBehaviour
{  
    public static BombBehaviour instance;

    [Header("Bomb Settings")]
    [SerializeField] private float fuseTime = 30f;
    private float currentTimer;

    [SerializeField] private float throwDistance = 10f;

    [Header("Explosion Effects")]
    [SerializeField] private float explosionForce = 10f;
    [SerializeField] private float explosionDuration = 1f;

    private NetworkObject networkObject;
    private GameObject currentHolder;
    private bool isActive = false;
    private Vector3 lastValidPosition;


    private bool bombPicked = false;

    private Rigidbody rb;

    BombManager bManager;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        networkObject = GetComponent<NetworkObject>();
        bManager = GameObject.FindObjectOfType<BombManager>().GetComponent<BombManager>();
        ResetBomb();

       
    }

    private void Update()
    {
        if (isActive && currentHolder != null)
        {
            bombPicked = true;

            currentTimer -= Time.deltaTime;
            lastValidPosition = currentHolder.transform.position;

            if (currentTimer <= 0)
            {
                Explode();
                bombPicked = false;
            }
        }
    }

    public void PickUp(GameObject vehicle)
    {
        //if (!IsServer) return; // Ensure only the server handles this

        currentHolder = vehicle;
        Transform bombHolder = vehicle.transform.Find("BombHolder");
        Debug.Log(bombHolder.transform.localPosition);

        if (bombHolder != null)
        {
            // Properly set parent using Netcode
            networkObject.TrySetParent(vehicle.GetComponent<NetworkObject>(), true);

            transform.localPosition = bombHolder.localPosition;
            transform.localRotation = Quaternion.identity;

            rb.isKinematic = true; // Prevent physics from affecting it
            rb.detectCollisions = false;
            Debug.Log("Bomb attached to BombHolder.");
        }
        else
        {
            Debug.LogWarning("BombHolder not found on vehicle! Using default position.");
            transform.position = vehicle.transform.position + new Vector3(0, 1.5f, 1.5f);
        }

        isActive = true;
        currentTimer = fuseTime;
        lastValidPosition = transform.position;
    }

    public void Throw()
    {
        //if (!IsServer) return; // Ensure only the server handles this

        if (currentHolder != null)
        {
            // Detach properly using Netcode
            networkObject.TrySetParent((NetworkObject)null, true);
            rb.isKinematic = true; // Enable physics again
            rb.detectCollisions = true;
            rb.AddForce(currentHolder.transform.forward * throwDistance, ForceMode.Impulse);

            currentHolder = null;
            ResetBomb();
        }
    }

    public void Explode()
    {

		if (currentHolder != null)
        {
            VehicleDebuff debuff = currentHolder.GetComponent<VehicleDebuff>();
			currentHolder.GetComponent<PlayerBombTrackerPointSystem>().DropBomb();

			debuff?.ApplyExplosion();

            networkObject.TrySetParent((NetworkObject)null, true);
            rb.isKinematic = false; // Enable physics again
            rb.detectCollisions = true;
            Destroy(gameObject);

            //transform.position = lastValidPosition;


        }

  //      if (bManager != null)
  //      {

  //          //bManager.SpawnBombOnRandomPlayer();
  //          Debug.Log("Asked BombManager To Respawn");

		//}
        

	}

	public void TransferToNewHolder(GameObject newHolder)
	{
		//if (!IsServer) return; // Ensure only the server handles this

		// Unparent from previous holder (if any)
		if (currentHolder != null)
		{
			networkObject.TrySetParent((NetworkObject)null, true);
		}

		currentHolder = newHolder;
		JankyCarControl_Multiplayer vehicleController = newHolder.GetComponent<JankyCarControl_Multiplayer>();

		if (vehicleController != null && vehicleController.bombHolder != null)
		{
			networkObject.TrySetParent(vehicleController.bombHolder.GetComponent<NetworkObject>(), true);
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;

			rb.isKinematic = true;
			rb.detectCollisions = false;

			Debug.Log("Bomb transferred successfully!");
		}
		else
		{
			Debug.LogWarning("New holder does not have a bomb holder setup!");
		}

		// Reactivate the bomb
		isActive = true;
		currentTimer = fuseTime;
	}

	private void OnTriggerEnter(Collider other)
	{
		//if (!IsServer) return; // Ensure only the server handles this

		if (other.CompareTag("BombWallCollider"))
		{
			Debug.Log("Bomb hit a wall! Respawning...");
            
            bManager.SpawnBombOnRandomPlayer();

        }

	}

	private void ResetBomb()
    {
        currentTimer = fuseTime;
        currentHolder = null;


    }




}




















//////////////////////////////////////////////////////////////////////////////////






//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using Unity.Netcode;

//public class BombBehaviour : NetworkBehaviour
//{
//	[Header("Bomb Settings")]
//	[SerializeField] private float fuseTime = 5f;
//	private float currentTimer;

//	[SerializeField] private float throwDistance = 10f;

//	[Header("Explosion Effects")]
//	[SerializeField] private float explosionForce = 10f;
//	[SerializeField] private float explosionDuration = 1f;

//	private NetworkObject networkObject;
//	private GameObject currentHolder;
//	private bool isActive = false;
//	private Vector3 lastValidPosition;

//	private Rigidbody rb;

//	private void Start()
//	{
//		rb = GetComponent<Rigidbody>();
//		networkObject = GetComponent<NetworkObject>();
//		lastValidPosition = transform.position; // Set initial position to prevent (0,0,0) issue
//		ResetBomb();

//	}

//	private void Update()
//	{
//		if (isActive && currentHolder != null)
//		{
//			currentTimer -= Time.deltaTime;
//			lastValidPosition = currentHolder.transform.position;

//			if (currentTimer <= 0)
//			{
//				Explode();
//			}
//		}
//	}

//	public void PickUp(GameObject vehicle)
//	{
//		if (!IsServer) return; // Ensure only the server handles this

//		currentHolder = vehicle;
//		JankyCarControl_Multiplayer vehicleController = vehicle.GetComponent<JankyCarControl_Multiplayer>();

//		if (vehicleController != null && vehicleController.bombHolder != null)
//		{
//			// Properly set parent using Netcode
//			networkObject.TrySetParent(vehicleController.bombHolder.GetComponent<NetworkObject>(), true);

//			transform.localPosition = Vector3.zero;
//			transform.localRotation = Quaternion.identity;

//			rb.isKinematic = true; // Prevent physics from affecting it
//			rb.detectCollisions = false;

//			Debug.Log("Bomb attached to BombHolder.");
//		}
//		else
//		{
//			Debug.LogWarning("BombHolder not assigned on vehicle! Using default position.");
//			transform.position = vehicle.transform.position + new Vector3(0, 1.5f, 1.5f);
//		}

//		isActive = true;
//		currentTimer = fuseTime;
//		lastValidPosition = transform.position;
//	}

//	public void Throw()
//	{
//		if (!IsServer) return; // Ensure only the server handles this

//		if (currentHolder != null)
//		{
//			Debug.Log("Throwing bomb...");

//			// Store the vehicle's forward direction before detaching
//			Vector3 throwDirection = currentHolder.transform.forward;

//			// De-parent the bomb
//			networkObject.TrySetParent((NetworkObject)null, true);

//			// Set bomb’s position to some units ahead in the vehicle's direction
//			transform.position = currentHolder.transform.position + throwDirection * 3f; // Adjust 3f as needed

//			Debug.Log("Bomb de-parented and teleported to: " + transform.position);

//			// Enable physics again
//			rb.isKinematic = false;
//			rb.detectCollisions = true;

//			// Apply forward force
//			rb.velocity = throwDirection * throwDistance; // Direct velocity instead of AddForce

//			Debug.Log("Bomb velocity after throw: " + rb.velocity);

//			// Clear the holder reference
//			currentHolder = null;
//		}
//	}

//	private void Explode()
//	{
//		if (currentHolder != null)
//		{
//			VehicleDebuff debuff = currentHolder.GetComponent<VehicleDebuff>();
//			debuff?.ApplyExplosion();

//			networkObject.TrySetParent((NetworkObject)null, true);
//			if (lastValidPosition != Vector3.zero)
//			{
//				transform.position = lastValidPosition;
//			}
//			else
//			{
//				Debug.LogWarning("Warning: lastValidPosition is zero, preventing teleport to (0,0,0)");
//			}
//			lastValidPosition = transform.position; // Set this as soon as it's picked up

//			Debug.Log("Bomb exploded! Dropped at last valid position: " + lastValidPosition);
//		}
//		ResetBomb();
//	}

//	private void ResetBomb()
//	{
//		isActive = false;
//		currentTimer = fuseTime;
//		currentHolder = null;
//	}
//}
