using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{















    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////Tut_2
    //public GameObject player;
    //public GameObject child;
    //public float speed;

    //private void Awake()
    //{
    //    player = GameObject.FindGameObjectWithTag("Player");
    //    child = player.transform.Find("camera constraint").gameObject;

    //}

    //private void FixedUpdate()
    //{
    //    Follow();
    //}

    //private void Follow()
    //{
    //    gameObject.transform.position = Vector3.Lerp(transform.position, child.transform.position, Time.deltaTime * speed);
    //    gameObject.transform.LookAt(player.gameObject.transform.position);
    //}











    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////Tut_1
    //internal enum updateMethod
    //{
    //    fixedUpdate,
    //    update,
    //    lateUpdate
    //}

    //[SerializeField]
    //private updateMethod updateDemo;

    //public GameObject cameraLookAtObject;
    //private SinglePlayerCarController controllerReference;

    //[Range(0, 20)]
    //public float smoothTime = 5f;

    //public Vector3 offset = new Vector3(0, 2, -5); // Adjust for height and distance behind the player

    //private void Start()
    //{
    //    controllerReference = GameObject.FindGameObjectWithTag("Player").GetComponent<SinglePlayerCarController>();
    //    cameraLookAtObject = GameObject.FindGameObjectWithTag("Player").transform.Find("camera lookAt").gameObject;
    //}

    //private void FixedUpdate()
    //{
    //    if (updateDemo == updateMethod.fixedUpdate)
    //    {
    //        CameraBehaviour();
    //    }
    //}

    //private void Update()
    //{
    //    if (updateDemo == updateMethod.update)
    //    {
    //        CameraBehaviour();
    //    }
    //}

    //private void LateUpdate()
    //{
    //    if (updateDemo == updateMethod.lateUpdate)
    //    {
    //        CameraBehaviour();
    //    }
    //}

    //private void CameraBehaviour()
    //{
    //    Vector3 velocity = Vector3.zero;
    //    Vector3 targetPosition = cameraLookAtObject.transform.position + cameraLookAtObject.transform.forward * offset.z + cameraLookAtObject.transform.up * offset.y;
    //    transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime * Time.deltaTime);
    //    transform.LookAt(cameraLookAtObject.transform);
    //}


}
