using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;

//using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class ExplosiveBarrel : NetworkBehaviour
{
    public GameObject explosionPrefab; // Assign your explosion GameObject in Inspector
    public float respawnTime = 5f; // Time in seconds before the barrel respawns

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float respawnTimer = 6f;
    public bool isRespawning = false;

    private void Start()
    {
        // Store the initial position and rotation of the barrel
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object has the tag "Player"
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Barrel exploded!"); // Log when barrel explodes

            // Spawn the explosion at the barrel's position
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }
            Invoke("RespawnBarrel", respawnTime);
            gameObject.SetActive(false);
        }
    }

    private void RespawnBarrel()
    {
        // Reset position and rotation
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // Re-enable the barrel
        gameObject.SetActive(true);

        // Reset respawn variables
        isRespawning = false;
        respawnTimer = 0f;

        Debug.Log("Barrel respawned!");
    }

}
    