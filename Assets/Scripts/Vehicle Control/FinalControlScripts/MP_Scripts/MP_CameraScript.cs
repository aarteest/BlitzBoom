using UnityEngine;
using Cinemachine;
using System.Buffers;

public class MP_CameraScript : MonoBehaviour
{
    private CinemachineVirtualCamera vCam;
    private Transform carTransform;
    public float rotationSpeed = 3f; // How fast the camera follows rotation

    void Start()
    {
        vCam = GetComponent<CinemachineVirtualCamera>();

        // Find all MP_HoverCarControl objects in the scene
        JankyCarControl_Multiplayer[] hoverCars = FindObjectsOfType<JankyCarControl_Multiplayer>();

        foreach (JankyCarControl_Multiplayer hoverCar in hoverCars)
        {
            // Check if the hover car is owned by this player
            if (hoverCar.IsOwner)  // Assuming MP_HoverCarControl has an IsOwner property
            {
                carTransform = hoverCar.transform;
                vCam.Follow = carTransform;
                vCam.LookAt = carTransform;
                break; // Stop searching after finding the owned car
            }
        }
    }

    void Update()
    {
        if (carTransform == null) return;

        // Get the target Y rotation (horizontal direction of the car)
        Quaternion targetRotation = Quaternion.Euler(0, carTransform.eulerAngles.y, 0);

        // Smoothly rotate towards the target rotation
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }


    //void Start()
    //{
    //    vCam = GetComponent<CinemachineVirtualCamera>();
    //    if (vCam.Follow != null)
    //        carTransform = vCam.Follow;



    //}

    //void Update()
    //{
    //    if (carTransform == null) return;

    //    // Get the target Y rotation (horizontal direction of the car)
    //    Quaternion targetRotation = Quaternion.Euler(0, carTransform.eulerAngles.y, 0);

    //    // Smoothly rotate towards the target rotation
    //    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    //}
}
