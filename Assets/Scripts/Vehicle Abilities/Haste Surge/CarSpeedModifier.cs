using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpeedModifier : MonoBehaviour
{
    private MultiplayerCarController car;
    private float originalMoveSpeed;

	// Reference to the AudioManager to play the Haste ability sound
	private AudioManager audioManager;

	private void Start()
    {
        car = GetComponent<MultiplayerCarController>();
        if (car != null)
        {
            originalMoveSpeed = car.moveSpeed;
        }

		// Get the AudioManager instance
		audioManager = AudioManager.Instance;
	}

    public void ModifySpeed(float multiplier, float duration)
    {
        if (car != null)
        {
            car.moveSpeed *= multiplier;

			// Play the Haste ability sound (use appropriate index for haste sound)
			audioManager.PlayAbilitySound(3);

			StartCoroutine(ResetSpeedAfterDuration(duration));
        }
    }

    private IEnumerator ResetSpeedAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (car != null)
        {
            car.moveSpeed = originalMoveSpeed;
        }

		// Stop the ability sound after the speed boost duration ends
		audioManager.StopAbilitySound();
	}
}
