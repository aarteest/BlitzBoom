using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedHaste : MonoBehaviour
{
    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // Prefab for the projectile
    public GameObject zonePrefab;      // Prefab for the zone
    public Transform firePoint;        // Point from which the projectile is fired
    public Transform targetPoint;      // Target point where the projectile will go
    public float projectileSpeed = 20f; // Speed of the projectile

    [Header("Zone Settings")]
    public float zoneDuration = 10f;   // Duration of the zone
    public float zoneEffectMultiplier = 100f; // Speed multiplier for the zone

    private Rigidbody rb;

    public float dashSpeed = 30f;


    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody not found on the car.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            FireProjectile();
        }
    }

    void FireProjectile()
    {
        // Instantiate and launch the projectile
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

        // Ensure the projectile has a Rigidbody
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = projectile.AddComponent<Rigidbody>();
        }

        rb.useGravity = false; // Disable gravity for the projectile

        // Calculate direction from firePoint to targetPoint
        Vector3 direction = (targetPoint.position - firePoint.position).normalized;

        // Apply velocity to the projectile
        rb.velocity = direction * projectileSpeed;

        // Attach behavior for collision handling
        ProjectileBehavior behavior = projectile.AddComponent<ProjectileBehavior>();
        behavior.zonePrefab = zonePrefab;
        behavior.zoneDuration = zoneDuration;
        behavior.zoneEffectMultiplier = zoneEffectMultiplier;
    }

    private class ProjectileBehavior : MonoBehaviour
    {
        public GameObject zonePrefab;      // Prefab for the zone
        public float zoneDuration;         // Duration of the zone
        public float zoneEffectMultiplier; // Effect multiplier for the zone
        private bool hasSpawnedZone = false;

        private void OnCollisionEnter(Collision collision)
        {
            if (!hasSpawnedZone)
            {
                SpawnZone();
            }
        }

        private void SpawnZone()
        {
            hasSpawnedZone = true; // Ensure the zone spawns only once

            // Spawn the zone at the collision point
            GameObject zone = Instantiate(zonePrefab, transform.position, Quaternion.identity);

            // Initialize the zone
            ZoneBehavior zoneBehavior = zone.AddComponent<ZoneBehavior>();
            zoneBehavior.Initialize(zoneDuration, zoneEffectMultiplier);

            // Destroy the projectile
            Destroy(gameObject);
        }
    }

    private class ZoneBehavior : MonoBehaviour
    {
        private float duration;            // Duration the zone will last
        private float effectMultiplier;    // Effect multiplier for the zone

        public void Initialize(float zoneDuration, float multiplier)
        {
            duration = zoneDuration;
            effectMultiplier = multiplier;
            Destroy(gameObject, duration); // Destroy the zone after the duration
        }

        //private void OnTriggerEnter(Collider other)
        //{
        //    // Check if the object is a car
        //    ActualFloatingCar car = other.GetComponent<ActualFloatingCar>();
        //    if (car != null)
        //    {
        //        car.ModifySpeed(effectMultiplier); // Adjust the car's speed
        //    }
        //}
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RocketBlaze"))
        {
            rb.velocity = transform.forward * dashSpeed;
        }
    }


}
