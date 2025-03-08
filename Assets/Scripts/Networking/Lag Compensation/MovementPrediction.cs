using Unity.Netcode;
using UnityEngine;

public class MovementPrediction : NetworkBehaviour
{
    private Vector3 lastServerPosition;
    private Vector3 predictedPosition;
    private float lerpSpeed = 10f;

    private void Start()
    {
        if (IsOwner)
        {
            lastServerPosition = transform.position;
            predictedPosition = transform.position;
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Predict next movement
        Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        predictedPosition += inputDirection * 5f * Time.deltaTime; // Assume constant speed

        // Interpolate between last known server position and predicted position
        transform.position = Vector3.Lerp(lastServerPosition, predictedPosition, Time.deltaTime * lerpSpeed);
    }

    [Rpc(SendTo.Server)]
    private void SendPositionToServerRpc(Vector3 position)
    {
        lastServerPosition = position;
    }
}