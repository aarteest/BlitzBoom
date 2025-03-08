using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LagCompensationManager : NetworkBehaviour
{
    private class PositionData
    {
        public Vector3 Position;
        public float Time;
    }

    private Dictionary<ulong, List<PositionData>> playerPositionHistory = new Dictionary<ulong, List<PositionData>>();
    private const float maxHistoryTime = 2.0f; // Store 2 seconds of position history

    private void Update()
    {
        if (!IsServer) return; // Only server stores positions

        float currentTime = Time.time;

        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            ulong clientId = client.Key;
            Transform playerTransform = client.Value.PlayerObject.transform;

            if (!playerPositionHistory.ContainsKey(clientId))
            {
                playerPositionHistory[clientId] = new List<PositionData>();
            }

            playerPositionHistory[clientId].Add(new PositionData
            {
                Position = playerTransform.position,
                Time = currentTime
            });

            // Remove old data beyond the history limit
            playerPositionHistory[clientId].RemoveAll(data => currentTime - data.Time > maxHistoryTime);
        }
    }

    public Vector3 GetRewoundPosition(ulong clientId, float rewindTime)
    {
        if (!playerPositionHistory.ContainsKey(clientId)) return Vector3.zero;

        List<PositionData> history = playerPositionHistory[clientId];

        for (int i = history.Count - 1; i >= 0; i--)
        {
            if (history[i].Time <= rewindTime)
            {
                return history[i].Position;
            }
        }

        return history.Count > 0 ? history[0].Position : Vector3.zero;
    }
}