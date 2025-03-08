using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashAbility : MonoBehaviour
{
    public GameObject blackVenom;  // The Black Venom GameObject
    private GameObject smokeTrail;

    [SerializeField]
    private float smokeTime = 5f;

	// Reference to the AudioManager to play and stop the Dash ability sound
	private AudioManager audioManager;


	private void Start()
    {
		// Get the AudioManager instance
		audioManager = AudioManager.Instance;

		// Find the SmokeTrail GameObject under Prometheus
		if (blackVenom != null)
        {
            smokeTrail = blackVenom.transform.Find("SmokeTrail").gameObject;

            // Ensure the SmokeTrail is disabled initially
            if (smokeTrail != null)
            {
                smokeTrail.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Ensure the player has the "Player" tag
        {
            if (smokeTrail != null)
            {
                smokeTrail.SetActive(true);  // Enable the SmokeTrail GameObject
                ParticleSystem smokeParticleSystem = smokeTrail.GetComponent<ParticleSystem>();
                if (smokeParticleSystem != null)
                {
                    smokeParticleSystem.Play();  // Play the smoke trail particle system
                }

				// Play the Dash ability sound (use appropriate index for dash sound)
				audioManager.PlayAbilitySound(4);

				// Start the coroutine to disable the SmokeTrail after 5 seconds
				StartCoroutine(DisableSmokeTrailAfterDelay());
            }
        }
    }

    // Coroutine to disable the smoke trail after a specified delay
    private IEnumerator DisableSmokeTrailAfterDelay()
    {
        yield return new WaitForSeconds(smokeTime);  // Wait for the specified delay

        if (smokeTrail != null)
        {
            smokeTrail.SetActive(false);  // Disable the SmokeTrail GameObject
            ParticleSystem smokeParticleSystem = smokeTrail.GetComponent<ParticleSystem>();
            if (smokeParticleSystem != null)
            {
                smokeParticleSystem.Stop();  // Stop the smoke trail particle system
            }
        }

		// Stop the Dash ability sound after the ability ends
		audioManager.StopAbilitySound();
	}

}
