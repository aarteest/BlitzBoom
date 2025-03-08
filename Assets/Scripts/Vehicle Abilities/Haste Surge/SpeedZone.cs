using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedZone : MonoBehaviour
{
    public float boostMultiplier = 2f; // Speed multiplier
    public float boostDuration = 2f;  // Duration of the speed boost


	private void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player car
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has entered the Speed Zone!"); // Log the collision

            Rigidbody playerRigidbody = other.GetComponent<Rigidbody>();
            if (playerRigidbody != null)
            {
                StartCoroutine(ApplySpeedBoost(playerRigidbody));
            }
        }
    }

    private IEnumerator ApplySpeedBoost(Rigidbody playerRigidbody)
    {
        float originalDrag = playerRigidbody.drag;  // Save the original drag
        playerRigidbody.drag /= boostMultiplier;    // Reduce drag to simulate speed boost

        Debug.Log("Speed boost applied!");          // Log speed boost activation

        yield return new WaitForSeconds(boostDuration);

        playerRigidbody.drag = originalDrag;        // Reset drag to original value
        Debug.Log("Speed boost ended.");           // Log the end of the boost
        gameObject.SetActive(false);               // Optionally disable the zone after use
    }
}
