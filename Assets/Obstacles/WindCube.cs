using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class WindCube : NetworkBehaviour
{
    public Vector3 moveDirection = Vector3.right;
    public float moveDistance = 5f;
    public float moveSpeed = 2f;
    public ParticleSystem windVFX; // VFX for visual effect

    private Vector3 startPosition;
    private Vector3 endPosition;
    private Windzone windZone;
    private bool hasTriggeredZone = false;

    void Start()
    {
        startPosition = transform.position;
        endPosition = startPosition + moveDirection.normalized * moveDistance;

        windZone = GetComponentInParent<Windzone>();

        if (windZone == null)
        {
            Debug.LogError("WindZone not found! Ensure WindCube is a child of WindZone.");
        }

        if (windVFX != null)
        {
            windVFX.Stop(); // Ensure VFX is off initially
        }
    }

    void Update()
    {
        if (!gameObject.activeSelf) return;

        transform.position = Vector3.MoveTowards(transform.position, endPosition, moveSpeed * Time.deltaTime);

        if (windVFX != null && !windVFX.isPlaying)
        {
            windVFX.Play(); // Activate VFX when moving
        }

        if (Vector3.Distance(transform.position, endPosition) < 0.1f && !hasTriggeredZone)
        {
            hasTriggeredZone = true;
            windZone?.DeactivateWindCubes();

            if (windVFX != null)
            {
                windVFX.Stop(); // Stop VFX when movement ends
            }
        }
    }

    public void ResetPosition()
    {
        transform.position = startPosition;
        hasTriggeredZone = false;

        if (windVFX != null)
        {
            windVFX.Stop(); // Reset VFX when repositioning
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            startPosition = transform.position;
            endPosition = startPosition + moveDirection.normalized * moveDistance;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawLine(startPosition, endPosition);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody carRb = other.GetComponent<Rigidbody>();

            if (carRb != null)
            {
                carRb.position += moveDirection.normalized * 100f + Vector3.up * 20f;
            }
        }
    }
}









///////////////////////////////////////////////////////////////


//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class WindCube : MonoBehaviour
//{
//    public Vector3 moveDirection = Vector3.right;
//    public float moveDistance = 5f;
//    public float moveSpeed = 2f;

//    private Vector3 startPosition;
//    private Vector3 endPosition;
//    private Windzone windZone;
//    private bool isDestroyed = false;

//    void Start()
//    {
//        startPosition = transform.position;
//        endPosition = startPosition + moveDirection.normalized * moveDistance;

//        // Find the WindZone in the scene
//        windZone = FindObjectOfType<Windzone>();

//        if (windZone == null)
//        {
//            Debug.LogError("WindZone not found! Make sure there's a Windzone component in the scene.");
//        }
//    }

//    void Update()
//    {
//        if (isDestroyed) return;

//        transform.position = Vector3.MoveTowards(transform.position, endPosition, moveSpeed * Time.deltaTime);

//        if (Vector3.Distance(transform.position, endPosition) < 0.1f)
//        {
//            isDestroyed = true;

//            if (windZone != null)
//            {
//                windZone.RespawnWindCube(startPosition);
//            }

//            // Delay destruction to avoid referencing destroyed object
//            Invoke(nameof(SafeDestroy), 0.1f);
//        }
//    }

//    private void SafeDestroy()
//    {
//        gameObject.SetActive(false); // Disable first to prevent reference errors
//        Destroy(gameObject);
//    }

//    private void OnDrawGizmos()
//    {
//        if (!Application.isPlaying)
//        {
//            startPosition = transform.position;
//            endPosition = startPosition + moveDirection.normalized * moveDistance;
//        }

//        Gizmos.color = Color.green;
//        Gizmos.DrawLine(startPosition, endPosition);
//    }
//}
