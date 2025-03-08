using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXEnabler : MonoBehaviour
{
    public ParticleSystem vfx; // Assign your VFX in the Inspector

    private void OnEnable()
    {
        if (vfx != null)
        {
            vfx.Play(); // Play the VFX
            Debug.Log("VFX Enabled!"); // Debug message
        }
    }
}
