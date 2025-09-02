using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerReleaseOnCollision : MonoBehaviour
{
    [Header("Release Settings")]
    public List<GameObject> releasePrefabs;    // List of all release prefabs
    public Transform spawnPoint;               // Point where items spawn
    public Vector3 spawnOffset = Vector3.zero; // Offset from the spawn point
    public float cooldownTime = 5f;            // Cooldown between releases
    public bool alternateReleases = true;      // Cycle through releases in order

    [Header("Dependencies")]
    public SpriteRenderer litFlowerRenderer;   // Reference to the lit flower's SpriteRenderer

    private bool isCooldown = false;           // Tracks cooldown state
    private int currentIndex = 0;              // Tracks current index in release list

    private void Start()
    {
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }

        if (litFlowerRenderer == null)
        {
            litFlowerRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (litFlowerRenderer == null)
        {
            Debug.LogError("LitFlowerRenderer is not assigned and could not be found in children!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && litFlowerRenderer != null && litFlowerRenderer.enabled && !isCooldown)
        {
            ReleaseItem();
            StartCoroutine(CooldownCoroutine());
        }
    }

    private void ReleaseItem()
    {
        if (releasePrefabs == null || releasePrefabs.Count == 0)
        {
            Debug.LogWarning("No release prefabs assigned!");
            return;
        }

        GameObject prefabToSpawn;

        if (alternateReleases)
        {
            prefabToSpawn = releasePrefabs[currentIndex];
            currentIndex = (currentIndex + 1) % releasePrefabs.Count;
        }
        else
        {
            prefabToSpawn = releasePrefabs[Random.Range(0, releasePrefabs.Count)];
        }

        if (prefabToSpawn != null)
        {
            Vector3 spawnPosition = spawnPoint.position + spawnOffset;
            Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            Debug.Log($"Released: {prefabToSpawn.name} at {spawnPosition}");
        }
    }

    private IEnumerator CooldownCoroutine()
    {
        isCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        isCooldown = false;
    }
}
