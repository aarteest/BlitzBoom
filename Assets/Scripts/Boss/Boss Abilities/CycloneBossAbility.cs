using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CycloneBossAbility : MonoBehaviour
{
	public float moveSpeed = 10f; // Speed of the cyclone
	public float zigZagFrequency = 2f; // Frequency of the zig-zag pattern
	public float zigZagAmplitude = 5f; // Amplitude of the zig-zag pattern
	public float effectRadius = 5f; // Radius of the cyclone's effect
	public float throwForce = 20f; // Force applied to players

	private Vector3 startPosition;
	private float time;

	void Start()
	{
		startPosition = transform.position;
	}

	void Update()
	{
		// Update time for zig-zag motion
		time += Time.deltaTime;

		// Zig-zag motion
		Vector3 offset = new Vector3(
			Mathf.Sin(time * zigZagFrequency) * zigZagAmplitude,
			0,
			moveSpeed * Time.deltaTime
		);
		transform.position += offset;


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // COMMENTED THIS FOR NO ERROR

        // Check for players in range
        //DetectAndThrowPlayers();
    }


    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    // COMMENTED THIS FOR NO ERROR

    //void DetectAndThrowPlayers()
    //{
    //	Collider[] hitColliders = Physics.OverlapSphere(transform.position, effectRadius);

    //	foreach (var hitCollider in hitColliders)
    //	{
    //		// Check for multiplayer car script
    //		ActualFloatingCar multiplayerCar = hitCollider.GetComponent<ActualFloatingCar>();

    //		// Check for single-player car script
    //		SinglePlayerCarController singleplayerCar = hitCollider.GetComponent<SinglePlayerCarController>();

    //		if (multiplayerCar != null || singleplayerCar != null)
    //		{
    //			Rigidbody playerRb = hitCollider.GetComponent<Rigidbody>();

    //			if (playerRb != null)
    //			{
    //				// Calculate direction to throw player towards the edge
    //				Vector3 throwDirection = (hitCollider.transform.position - transform.position).normalized;
    //				throwDirection.y = 0; // Keep the force on the horizontal plane

    //				// Apply force
    //				playerRb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
    //			}
    //		}
    //	}
    //}

    void OnDrawGizmos()
	{
		// Visualize the cyclone's effect radius in the editor
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, effectRadius);
	}
}
