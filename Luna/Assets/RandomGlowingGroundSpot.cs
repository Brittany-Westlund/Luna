using UnityEngine;
using System.Collections;

public class RandomGlowingGroundSpot : MonoBehaviour
{
    [Header("Flower Settings")]
    public GameObject[] flowerPrefabs;   // Array of flower prefabs
    public Transform spawnPoint;
    public float restDuration = 3f;      // Time Luna must rest
    public float cooldown = 5f;          // Time before spot can be reused

    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public Collider2D triggerCollider;

    private bool isOccupied = false;
    private Coroutine restCoroutine;

    void Awake()
    {
        if (spawnPoint == null) spawnPoint = transform;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (triggerCollider == null) triggerCollider = GetComponent<Collider2D>();
    }

    void OnTriggerStay2D(Collider2D other)
    {
        var rest = other.GetComponent<LunaRest>();
        if (rest != null && rest.isResting && !isOccupied && restCoroutine == null)
        {
            restCoroutine = StartCoroutine(WaitAndSpawn(rest));
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // Cancel if Luna leaves before finishing
        if (restCoroutine != null)
        {
            StopCoroutine(restCoroutine);
            restCoroutine = null;
        }
    }

    private IEnumerator WaitAndSpawn(LunaRest rest)
    {
        float elapsed = 0f;

        while (elapsed < restDuration)
        {
            if (!rest.isResting) yield break; // Cancel if she stops
            elapsed += Time.deltaTime;
            yield return null;
        }

        SpawnFlower();
    }

    private void SpawnFlower()
    {
        if (flowerPrefabs != null && flowerPrefabs.Length > 0 && spawnPoint != null)
        {
            int randomIndex = Random.Range(0, flowerPrefabs.Length);
            GameObject chosenFlower = flowerPrefabs[randomIndex];

            Instantiate(chosenFlower, spawnPoint.position, Quaternion.identity);
        }

        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        isOccupied = true;

        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (triggerCollider != null) triggerCollider.enabled = false;

        yield return new WaitForSeconds(cooldown);

        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (triggerCollider != null) triggerCollider.enabled = true;

        isOccupied = false;
        restCoroutine = null;
    }
}
