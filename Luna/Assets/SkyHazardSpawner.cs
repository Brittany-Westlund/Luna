using UnityEngine;

public class SkyHazardSpawner : MonoBehaviour
{
    [Header("Hazard Prefabs (Add Multiple)")]
    public GameObject[] fallingHazardPrefabs;

    [Header("Spawn Area")]
    public float minX = -12f;
    public float maxX = 12f;
    public float spawnY = 10f;

    [Header("Timing")]
    public float minSpawnDelay = 0.3f;
    public float maxSpawnDelay = 1.0f;

    [Header("Burst Control")]
    public bool spawnOnStart = true;
    public bool keepSpawning = true;

    [Header("Optional Variation")]
    public Vector2 scaleRange = new Vector2(1f, 1f); // set to (0.8, 1.2) for variation
    public Vector2 fallSpeedRange = new Vector2(3f, 6f); // requires Rigidbody2D

    private float _nextSpawnTime;

    void Start()
    {
        ScheduleNextSpawn();

        if (spawnOnStart)
        {
            SpawnHazard();
        }
    }

    void Update()
    {
        if (!keepSpawning)
        {
            return;
        }

        if (Time.time >= _nextSpawnTime)
        {
            SpawnHazard();
            ScheduleNextSpawn();
        }
    }

    void SpawnHazard()
    {
        if (fallingHazardPrefabs == null || fallingHazardPrefabs.Length == 0)
        {
            Debug.LogWarning("No hazard prefabs assigned!");
            return;
        }

        // Pick random prefab from array
        GameObject prefab = fallingHazardPrefabs[Random.Range(0, fallingHazardPrefabs.Length)];

        float randomX = Random.Range(minX, maxX);
        Vector3 spawnPosition = new Vector3(randomX, spawnY, 0f);

        GameObject instance = Instantiate(prefab, spawnPosition, Quaternion.identity);

        // Optional: random scale variation
        float randomScale = Random.Range(scaleRange.x, scaleRange.y);
        instance.transform.localScale *= randomScale;

        // Optional: random fall speed
       Rigidbody2D rb = instance.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            float speed = Random.Range(fallSpeedRange.x, fallSpeedRange.y);
            rb.velocity = Vector2.down * speed;
        }
    }

    void ScheduleNextSpawn()
    {
        _nextSpawnTime = Time.time + Random.Range(minSpawnDelay, maxSpawnDelay);
    }
}