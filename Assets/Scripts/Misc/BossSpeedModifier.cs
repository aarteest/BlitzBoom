using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BossSpeedModifier : MonoBehaviour
{
    public static BossSpeedModifier Instance;

    public float bossSpeed = 10f; // Default speed

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keep this object across scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetBossSpeed(string speedInput)
    {
        if (float.TryParse(speedInput, out float speed))
        {
            bossSpeed = Mathf.Clamp(speed, 50f, 300f); // Prevents extreme values
        }
    }

    public float GetBossSpeed()
    {
        return bossSpeed;
    }
}
