using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoonbowFlowerSpawner : MonoBehaviour
{
    [Header("🌸 Flower Settings")]
    public List<GameObject> flowerPrefabs;

    [Tooltip("Primary spawn point (Moonbow start).")]
    public Transform spawnPointA;

    [Tooltip("Secondary spawn point (Moonbow end).")]
    public Transform spawnPointB;

    [Tooltip("Seconds after Moonbow appears before the flowers spawn.")]
    public float spawnDelay = 2f;

    [Tooltip("Vertical offset (lift flowers above ground).")]
    public float verticalOffset = 0f;

    [Header("Debug / Visuals")]
    public bool debugLogs = true;
    private bool hasSpawned = false;

    private void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(SpawnAfterDelay());
    }

    private IEnumerator SpawnAfterDelay()
    {
        yield return new WaitForSeconds(spawnDelay);
        SpawnAtPoint(spawnPointA, "A");
        SpawnAtPoint(spawnPointB, "B");

        hasSpawned = true;
    }

    private void SpawnAtPoint(Transform point, string label)
    {
        if (point == null) {
            if (debugLogs)
                Debug.LogWarning($"[MoonbowFlowerSpawner] SpawnPoint {label} is missing!");
            return;
        }

        // pick a random prefab
        GameObject prefab = flowerPrefabs[Random.Range(0, flowerPrefabs.Count)];

        // compute spawn location
        Vector3 pos = point.position + Vector3.up * verticalOffset;

        // instantiate
        Instantiate(prefab, pos, Quaternion.identity);

        if (debugLogs)
            Debug.Log($"🌸 Spawned {prefab.name} at point {label} → {pos}");
    }
}


/* using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoonbowFlowerSpawner : MonoBehaviour
{
    [Header("🌸 Flower Settings")]
    [Tooltip("List of flower prefabs to choose from randomly.")]
    public List<GameObject> flowerPrefabs;

    [Tooltip("Optional: where the flower should appear. Defaults to Moonbow position.")]
    public Transform spawnPoint;

    [Tooltip("Seconds after Moonbow appears before the flower spawns.")]
    public float spawnDelay = 2f;

    [Tooltip("Vertical offset (e.g., 0.1 lifts the flower above the ground).")]
    public float verticalOffset = 0f;

    [Header("Debug / Visuals")]
    public bool debugLogs = true;
    private bool hasSpawned = false;

    private void OnEnable()
    {
        if (spawnPoint == null) spawnPoint = transform;

        // restart coroutine every time the Moonbow appears
        StopAllCoroutines();
        StartCoroutine(SpawnAfterDelay());
    }

    private IEnumerator SpawnAfterDelay()
    {
        yield return new WaitForSeconds(spawnDelay);
        SpawnFlower();
    }

    private void SpawnFlower()
    {
        if (hasSpawned) return;

        if (flowerPrefabs == null || flowerPrefabs.Count == 0)
        {
            Debug.LogWarning("[MoonbowFlowerSpawner] No flower prefabs assigned!");
            return;
        }

        // Pick a random flower prefab
        GameObject prefab = flowerPrefabs[Random.Range(0, flowerPrefabs.Count)];

        // Spawn position
        Vector3 pos = spawnPoint.position + Vector3.up * verticalOffset;

        // Instantiate flower
        Instantiate(prefab, pos, Quaternion.identity);

        if (debugLogs)
            Debug.Log($"🌸 [MoonbowFlowerSpawner] Spawned {prefab.name} at {pos}");

        hasSpawned = true;
    }
}
*/