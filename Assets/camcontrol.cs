using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class camcontrol : MonoBehaviour
{
    public float moveSpeed = 5f;  // Speed of movement
    public float lookSpeed = 2f;  // Mouse sensitivity

    private float rotationX = 0f;
    private float rotationY = 0f;

    void Start()
    {
        // Lock and hide the cursor for better camera control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Get mouse movement
        rotationX += Input.GetAxis("Mouse X") * lookSpeed;
        rotationY -= Input.GetAxis("Mouse Y") * lookSpeed;
        rotationY = Mathf.Clamp(rotationY, -90f, 90f); // Prevent flipping

        transform.rotation = Quaternion.Euler(rotationY, rotationX, 0f);

        // Get keyboard movement
        Vector3 moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection = transform.TransformDirection(moveDirection); // Move relative to camera rotation
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // Unlock cursor with Escape key
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
