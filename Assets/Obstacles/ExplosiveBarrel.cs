using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;

//using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class ExplosiveBarrel : NetworkBehaviour
{
    public GameObject explosionPrefab; // Assign your explosion GameObject in Inspector

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the object has the tag "Car" or "Ground"
        if (collision.gameObject.CompareTag("Player"))
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
