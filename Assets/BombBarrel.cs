using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BombBarrel : NetworkBehaviour
{

    public GameObject explosionPrefab; // Assign your explosion GameObject in Inspector

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object has the tag "Car" or "Ground"
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Ground"))
        {
            // Spawn the explosion at the barrel's position
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            }

            // Destroy the barrel
            Destroy(gameObject);
        }
    }

}
