using UnityEngine;
using Cinemachine;

public class CameraScript : MonoBehaviour
{
    private CinemachineVirtualCamera vCam;
    private Transform carTransform;
    public float rotationSpeed = 3f; // How fast the camera follows rotation

    void Start()
    {
        vCam = GetComponent<CinemachineVirtualCamera>();
        if (vCam.Follow != null)
            carTransform = vCam.Follow;
    }

    void Update()
    {
        if (carTransform == null) return;

        // Get the target Y rotation (horizontal direction of the car)
        Quaternion targetRotation = Quaternion.Euler(0, carTransform.eulerAngles.y, 0);

        // Smoothly rotate towards the target rotation
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}
