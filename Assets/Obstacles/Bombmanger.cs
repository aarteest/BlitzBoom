using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Bombmanger : NetworkBehaviour
{

    public GameObject barrelPrefab; // Barrel prefab
    public GameObject warningPrefab; // Crosshair warning prefab
    public BoxCollider spawnAreaCollider; // Assign a BoxCollider in Unity
    public int barrelsPerWave = 5; // Number of barrels per wave
    public float warningDuration = 2f; // How long the warning appears before falling
    public float timeBetweenWaves = 5f; // Time before the next wave starts
    public Vector3 maxWarningSize = new Vector3(3f, 3f, 3f); // Max size of crosshair warning

    private void Start()
    {
        StartCoroutine(WaveSpawner());
    }

    private IEnumerator WaveSpawner()
    {
        while (true) // Keeps spawning waves indefinitely
        {
            SpawnBarrels();
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    public void SpawnBarrels()
    {
        if (spawnAreaCollider == null)
        {
            Debug.LogError("Spawn area collider is not assigned!");
            return;
        }

        for (int i = 0; i < barrelsPerWave; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            StartCoroutine(SpawnWithWarning(spawnPosition));
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Transform colliderTransform = spawnAreaCollider.transform;
        Vector3 center = spawnAreaCollider.center;
        Vector3 size = spawnAreaCollider.size;

        Vector3 localSpawnPosition = new Vector3(
            Random.Range(center.x - size.x / 2, center.x + size.x / 2),
            center.y + size.y / 2, // Spawn at the TOP face
            Random.Range(center.z - size.z / 2, center.z + size.z / 2)
        );

        return colliderTransform.TransformPoint(localSpawnPosition);
    }

    private IEnumerator SpawnWithWarning(Vector3 spawnPosition)
    {
        Vector3 groundPosition = spawnPosition;
        groundPosition.y -= spawnAreaCollider.size.y; // Move to ground level

        // Spawn warning crosshair
        GameObject warningMarker = Instantiate(warningPrefab, groundPosition, Quaternion.identity);
        warningMarker.transform.localScale = Vector3.zero; // Start small

        float elapsedTime = 0f;
        while (elapsedTime < warningDuration)
        {
            float scaleFactor = elapsedTime / warningDuration; // Lerp factor (0 to 1)
            warningMarker.transform.localScale = Vector3.Lerp(Vector3.zero, maxWarningSize, scaleFactor);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Remove warning marker before the barrel falls
        Destroy(warningMarker);

        // Spawn the barrel
        GameObject barrel = Instantiate(barrelPrefab, spawnPosition, Quaternion.identity);
        barrel.GetComponent<Rigidbody>().isKinematic = false; // Enable falling
    }

    private void OnDrawGizmos()
    {
        if (spawnAreaCollider != null)
        {
            Gizmos.color = Color.red;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(spawnAreaCollider.transform.position, spawnAreaCollider.transform.rotation, spawnAreaCollider.transform.lossyScale);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(spawnAreaCollider.center, spawnAreaCollider.size);
        }
    }
}
