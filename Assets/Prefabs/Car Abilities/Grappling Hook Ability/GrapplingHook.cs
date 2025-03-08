using System.Collections;
using UnityEngine;

public class GrapplingHook : MonoBehaviour
{
	public Transform playerCar;       // The car's Transform
	public Transform boss;           // The boss's Transform
	public LineRenderer grapplingLine; // The LineRenderer for the grappling hook
	public float grapplingRange = 30f; // Maximum distance to grapple
	public float hookSpeed = 50f;     // Speed at which the car moves toward the boss
	public float dashSpeed = 100f;    // Speed of the final dash toward the boss
	public float dashDuration = 0.5f; // Duration of the dash
	public float cooldownTime = 5f;   // Cooldown time for the grappling hook

	private bool isGrappling = false; // Is the grappling hook currently active?
	private Vector3 grapplePoint;     // The point where the grappling hook latched
	private float lastGrappleTime;    // Time of the last grapple

	// Reference to the AudioManager to play and stop the Dash ability sound
	private AudioManager audioManager;


	void Start()
	{
		// Get the AudioManager instance
		audioManager = AudioManager.Instance;
	}

	void Update()
	{
		// Only allow grappling if not on cooldown
		if (!isGrappling && Time.time >= lastGrappleTime + cooldownTime)
		{
			float distanceToBoss = Vector3.Distance(playerCar.position, boss.position);

			// Check if boss is in range and player presses the grapple button (E key)
			if (distanceToBoss <= grapplingRange && Input.GetKeyDown(KeyCode.E))
			{
				StartGrappling();
			}
		}

		// If grappling is active, move the car toward the grapple point
		if (isGrappling)
		{
			PerformGrappling();
		}
	}

	void StartGrappling()
	{
		isGrappling = true;
		grapplePoint = boss.position; // Latch onto the boss's current position
		grapplingLine.enabled = true;

		// Set LineRenderer positions for the grappling hook
		grapplingLine.SetPosition(0, playerCar.position); // Start at the car
		grapplingLine.SetPosition(1, grapplePoint);       // End at the boss

		// Play the grappling hook sound (Element 2, for example)
		audioManager.PlayAbilitySound(1);
	}

	void PerformGrappling()
	{
		// Smoothly move the car toward the grapple point
		playerCar.position = Vector3.MoveTowards(playerCar.position, grapplePoint, hookSpeed * Time.deltaTime);

		// Update LineRenderer positions as the car moves
		grapplingLine.SetPosition(0, playerCar.position);

		// Stop grappling if the car is close enough to the boss
		if (Vector3.Distance(playerCar.position, grapplePoint) < 1f)
		{
			StopGrappling();
		}
	}

	void StopGrappling()
	{
		isGrappling = false;
		grapplingLine.enabled = false; // Disable the grappling hook's visual

		// Apply a final dash toward the boss
		StartCoroutine(DashTowardsBoss());

		// Stop the grappling hook sound after finishing
		audioManager.StopAbilitySound();

		// Set the cooldown time
		lastGrappleTime = Time.time;
	}

	IEnumerator DashTowardsBoss()
	{
		float startTime = Time.time;

		while (Time.time < startTime + dashDuration)
		{
			playerCar.position = Vector3.MoveTowards(playerCar.position, grapplePoint, dashSpeed * Time.deltaTime);
			yield return null;
		}
	}
}
