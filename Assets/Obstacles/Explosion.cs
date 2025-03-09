using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Explosion : NetworkBehaviour
{
    public float explosionForce = 80000f; // Stronger force for big mass
    public float explosionRadius = 5f;    // Keeps force concentrated
    public float explosionUpwardModifier = 50000f; // Extra upward push
    public float explosionDuration = 2f;  // Auto-destroy time

    private void Start()
    {
        // Destroy explosion effect after a delay
        Destroy(gameObject, explosionDuration);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.gameObject.CompareTag("ExplosionBarrel")) // Make sure your car has the "Car" tag!
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();

            if (rb != null)
            {
                // Apply explosion force
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 1f, ForceMode.Impulse);

                // Apply additional vertical force
                rb.AddForce(Vector3.up * explosionUpwardModifier, ForceMode.Impulse);
            }
        }
    }
}
