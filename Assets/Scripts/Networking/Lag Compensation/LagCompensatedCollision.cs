using Unity.Netcode;
using UnityEngine;

public class LagCompensatedCollision : NetworkBehaviour
{
    private LagCompensationManager lagCompensationManager;
    private float clientPing = 0.1f; // Simulating 100ms ping

    private void Start()
    {
        if (IsServer)
        {
            lagCompensationManager = FindObjectOfType<LagCompensationManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; // Collision handled on server

        if (other.TryGetComponent<NetworkObject>(out NetworkObject otherObject))
        {
            ulong clientId = otherObject.OwnerClientId;

            // Rewind position based on estimated ping
            float rewindTime = Time.time - clientPing;
            Vector3 correctedPosition = lagCompensationManager.GetRewoundPosition(clientId, rewindTime);

            Debug.Log($"Rewound position for {clientId}: {correctedPosition}");
        }
    }
}