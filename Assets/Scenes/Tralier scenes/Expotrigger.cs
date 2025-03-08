using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Expotrigger : MonoBehaviour
{
    public ParticleSystem vfx; // Assign your VFX in the Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tralierstuff(olny adi)")) // Checks if the camera enters
        {
            Debug.Log("BOOM! Camera hit the trigger!");

            if (vfx != null)
            {
                vfx.Play(); // Play the VFX

            }
        }
    }
}
