using UnityEngine;

public class AudioManager : MonoBehaviour
{
	public static AudioManager Instance;

	public AudioSource bgMusicSource;   // For Background Music
	public AudioSource abilitySoundSource; // For Vehicle Abilities (reused for all abilities)

	public AudioClip bgMusicClip; // Background music clip
	public AudioClip[] vehicleAbilitySounds; // Array to hold the 6 ability sound clips

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject); // Keeps AudioManager between scene loads
		}
		else
		{
			Destroy(gameObject); // Ensures only one instance of AudioManager exists
		}
	}

	// Play the background music (loops across scenes)
	public void PlayBackgroundMusic()
	{
		if (!bgMusicSource.isPlaying)
		{
			bgMusicSource.clip = bgMusicClip;
			bgMusicSource.loop = true;
			bgMusicSource.Play();
			Debug.Log("Background music should be playing now.");
		}
		else
		{
			Debug.Log("Background music is already playing.");
		}
	}

	// Stop the background music
	public void StopBackgroundMusic()
	{
		bgMusicSource.Stop();
	}

	// Play ability sound (pass the index of the ability)
	public void PlayAbilitySound(int abilityIndex)
	{
		if (abilityIndex >= 0 && abilityIndex < vehicleAbilitySounds.Length)
		{
			abilitySoundSource.clip = vehicleAbilitySounds[abilityIndex];
			abilitySoundSource.Play();
		}
	}

	// Stop ability sound when ability ends
	public void StopAbilitySound()
	{
		abilitySoundSource.Stop();
	}
}
