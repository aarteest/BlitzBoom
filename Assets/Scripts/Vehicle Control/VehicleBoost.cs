using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class VehicleBoost : NetworkBehaviour
{
    public float boostForce = 5000f; // Adjust based on game physics
    public float boostDuration = 1.5f;
    public float boostCooldown = 3.0f;
    public GameObject boostVFXPrefab; // Prefab for boost VFX

    private Rigidbody rb;
    private bool isBoosting = false;
    private float boostEndTime = 0f;
    private float nextBoostTime = 0f;
    private GameObject activeBoostVFX;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!IsOwner) return; // Only local player can trigger boost

        if (Time.time >= nextBoostTime && (Input.GetKeyDown(KeyCode.LeftShift)) || Time.time >= nextBoostTime && (Input.GetButtonDown("ControllerBoost")))
        {
            StartBoostServerRpc();
        }
    }

    void FixedUpdate()
    {
        if (isBoosting)
        {
            rb.AddForce(transform.forward * boostForce * Time.fixedDeltaTime, ForceMode.Acceleration);

            if (Time.time >= boostEndTime)
            {
                EndBoostServerRpc();
            }
        }
    }

    [ServerRpc]
    void StartBoostServerRpc()
    {
        StartBoost();
        StartBoostClientRpc();
    }

    [ClientRpc]
    void StartBoostClientRpc()
    {
        if (!IsOwner) StartBoost(); // Prevents duplicate execution for local player
    }

    void StartBoost()
    {
        isBoosting = true;
        boostEndTime = Time.time + boostDuration;
        nextBoostTime = Time.time + boostCooldown;

        if (boostVFXPrefab != null && activeBoostVFX == null)
        {
            activeBoostVFX = Instantiate(boostVFXPrefab, transform.position, Quaternion.identity, transform);
        }
    }

    [ServerRpc]
    void EndBoostServerRpc()
    {
        EndBoost();
        EndBoostClientRpc();
    }

    [ClientRpc]
    void EndBoostClientRpc()
    {
        if (!IsOwner) EndBoost();
    }

    void EndBoost()
    {
        isBoosting = false;
        if (activeBoostVFX != null)
        {
            Destroy(activeBoostVFX);
            activeBoostVFX = null;
        }
    }

    // New Method to Reset Boost When Respawning
    public void ResetBoost()
    {
        isBoosting = false;
        boostEndTime = 0f;
        nextBoostTime = Time.time; // Reset cooldown

        if (activeBoostVFX != null)
        {
            Destroy(activeBoostVFX);
            activeBoostVFX = null;
        }
    }

    // Call This When Respawning to Reset Boost on All Clients
    [ServerRpc]
    public void OnRespawnServerRpc()
    {
        ResetBoostClientRpc();
    }

    [ClientRpc]
    void ResetBoostClientRpc()
    {
        ResetBoost();
    }
}










//using System.Collections;
//using System.Collections.Generic;
//using System.Globalization;
//using Unity.Netcode;
//using UnityEngine;




//public class VehicleBoost : NetworkBehaviour
//{
//    public float boostForce = 5000f; // Adjust based on your game physics
//    public float boostDuration = 1.5f;
//    public float boostCooldown = 3.0f;
//    public GameObject boostVFXPrefab; // Prefab for the boost VFX

//    private Rigidbody rb;
//    private bool isBoosting = false;
//    private float boostEndTime = 0f;
//    private float nextBoostTime = 0f;
//    private GameObject activeBoostVFX;

//    void Start()
//    {
//        rb = GetComponent<Rigidbody>();
//    }

//    void Update()
//    {
//        if (!IsOwner) return; // Ensures only the local player can trigger boost

//        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= nextBoostTime)
//        {
//            StartBoostServerRpc();
//        }
//    }

//    void FixedUpdate()
//    {
//        if (isBoosting)
//        {
//            rb.AddForce(transform.forward * boostForce * Time.fixedDeltaTime, ForceMode.Acceleration);

//            if (Time.time >= boostEndTime)
//            {
//                EndBoostServerRpc();
//            }
//        }
//    }

//    [ServerRpc]
//    void StartBoostServerRpc()
//    {
//        StartBoost();
//        StartBoostClientRpc();
//    }

//    [ClientRpc]
//    void StartBoostClientRpc()
//    {
//        if (!IsOwner) StartBoost(); // Prevents duplicate execution for local player
//    }

//    void StartBoost()
//    {
//        isBoosting = true;
//        boostEndTime = Time.time + boostDuration;
//        nextBoostTime = Time.time + boostCooldown;

//        if (boostVFXPrefab != null && activeBoostVFX == null)
//        {
//            activeBoostVFX = Instantiate(boostVFXPrefab, transform.position, Quaternion.identity, transform);
//        }
//    }

//    [ServerRpc]
//    void EndBoostServerRpc()
//    {
//        EndBoost();
//        EndBoostClientRpc();
//    }

//    [ClientRpc]
//    void EndBoostClientRpc()
//    {
//        if (!IsOwner) EndBoost();
//    }

//    void EndBoost()
//    {
//        isBoosting = false;
//        if (activeBoostVFX != null)
//        {
//            Destroy(activeBoostVFX);
//            activeBoostVFX = null;
//        }
//    }
//}






//public class VehicleBoost : MonoBehaviour
//{
//	public float boostForce = 5000f; // Adjust based on your game physics
//	public float boostDuration = 1.5f;
//	public float boostCooldown = 3.0f;
//	public GameObject boostVFXPrefab; // Prefab for the boost VFX

//	private Rigidbody rb;
//	private bool isBoosting = false;
//	private float boostEndTime = 0f;
//	private float nextBoostTime = 0f;
//	private GameObject activeBoostVFX;

//	void Start()
//	{
//		rb = GetComponent<Rigidbody>();
//	}

//	void Update()
//	{
//		if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= nextBoostTime)
//		{
//			StartBoost();
//		}
//	}

//	void FixedUpdate()
//	{
//		if (isBoosting)
//		{
//			rb.AddForce(transform.forward * boostForce * Time.fixedDeltaTime, ForceMode.Acceleration);

//			if (Time.time >= boostEndTime)
//			{
//				EndBoost();
//			}
//		}
//	}

//	void StartBoost()
//	{
//		isBoosting = true;
//		boostEndTime = Time.time + boostDuration;
//		nextBoostTime = Time.time + boostCooldown;
//		if (boostVFXPrefab != null && activeBoostVFX == null)
//		{
//			activeBoostVFX = Instantiate(boostVFXPrefab, transform.position, Quaternion.identity, transform);
//		}
//	}

//	void EndBoost()
//	{
//		isBoosting = false;
//		if (activeBoostVFX != null)
//		{
//			Destroy(activeBoostVFX);
//			activeBoostVFX = null;
//		}
//	}
//}
