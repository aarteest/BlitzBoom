using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraObject : MonoBehaviour
{
    public GameObject carToFollow;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (carToFollow == null) return;

        Vector3 targetPos = carToFollow.transform.position;
        rb.MovePosition(Vector3.Lerp(rb.position, targetPos, 0.5f));
    }
}
