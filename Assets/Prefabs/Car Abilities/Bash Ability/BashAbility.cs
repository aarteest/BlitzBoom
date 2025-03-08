using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BashAbility : MonoBehaviour
{
	public float reboundForce = 500f;
	public float abilityDuration = 5f;
	public float maxSpeed = 20f;
	private float currentSpeed = 0f;
	private Rigidbody rb;
	private bool isAbilityActive = false;
	private float abilityEndTime;

	private MultiplayerCarController carCore;

	// Reference to the AudioManager to play and stop the Dash ability sound
	private AudioManager audioManager;

	void Start()
	{
		// Get the AudioManager instance
		audioManager = AudioManager.Instance;


		rb = GetComponent<Rigidbody>();
		carCore = GetComponent<MultiplayerCarController>();
		if (carCore == null)
		{
			Debug.LogError("MultiplayerCarController script not found on the GameObject!");
		}
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.LeftControl) && !isAbilityActive)
		{
			ActivateReboundAbility();
		}

		if (isAbilityActive)
		{
			float horizontalInput = Input.GetAxis("Horizontal");
			Vector3 horizontalMovement = new Vector3(horizontalInput, 0, 0);

			rb.velocity = new Vector3(horizontalMovement.x * carCore.moveSpeed, rb.velocity.y, currentSpeed);

			if (Time.time >= abilityEndTime)
			{
				DeactivateReboundAbility();
			}
		}
	}

	private void ActivateReboundAbility()
	{
		isAbilityActive = true;
		abilityEndTime = Time.time + abilityDuration;
		currentSpeed = carCore.moveSpeed;
		Debug.Log("Ability activated");

		// Play the Dash ability sound (use appropriate index for dash sound)
		audioManager.PlayAbilitySound(5);

	}

	private void DeactivateReboundAbility()
	{
		isAbilityActive = false;
		currentSpeed = 0f;
		rb.angularVelocity = Vector3.zero;
		Debug.Log("Ability ended");

		// Stop the Dash ability sound after the ability ends
		audioManager.StopAbilitySound();
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (isAbilityActive && collision.gameObject.CompareTag("Boundary"))
		{
			Vector3 reboundDirection = Vector3.Reflect(rb.velocity.normalized, collision.contacts[0].normal);
			float impactForce = Mathf.Clamp(rb.velocity.magnitude, 0, 10f);
			rb.AddForce(reboundDirection * impactForce * reboundForce, ForceMode.Impulse);
		}
	}

	void FixedUpdate()
	{
		rb.angularVelocity = Vector3.ClampMagnitude(rb.angularVelocity, 5f);
	}
}
