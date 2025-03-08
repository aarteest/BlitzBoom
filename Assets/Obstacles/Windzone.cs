using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Unity.Netcode;

public class Windzone : NetworkBehaviour
{
    public WindCube windCube; // Reference to the WindCube managed by this Windzone
    public float respawnDelay = 2f; // Time before reactivation

    private void Start()
    {
        if (windCube == null)
        {
            windCube = GetComponentInChildren<WindCube>(); // Automatically finds a WindCube if not assigned
        }

        if (windCube == null)
        {
            Debug.LogError("No WindCube assigned or found in children of WindZone!");
            return;
        }
    }

    public void DeactivateWindCubes()
    {
        if (windCube == null) return;

        windCube.gameObject.SetActive(false); // Deactivate WindCube
        StartCoroutine(ReactivateWindCube()); // Schedule reactivation
    }

    private IEnumerator ReactivateWindCube()
    {
        yield return new WaitForSeconds(respawnDelay); // Wait before reactivating
        windCube.ResetPosition(); // Reset position before activation
        windCube.gameObject.SetActive(true); // Reactivate WindCube
    }
}
