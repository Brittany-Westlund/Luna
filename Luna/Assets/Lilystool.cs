using UnityEngine;
using System.Collections;

public class LilyStool : MonoBehaviour
{
    public Transform teapotSpawnPoint;

    [Header("Gate State")]
    [SerializeField] private bool playerOnLilypad = false;
    public bool PlayerOnLilypad => playerOnLilypad;

    [Header("Auto Teapot Settings")]
    [SerializeField] private GameObject teapotPrefab;
    [SerializeField] private float spawnDelay = 2f;

    [Header("Teacup Gate")]
    [SerializeField] private string teacupTag = "Teacup";
    [SerializeField] private bool retryWhileBlocked = true;
    [SerializeField] private float retryDelay = 1f;

    [Header("Post-Brew Spawn Block")]
    [SerializeField] private float postBrewSpawnBlockTime = 0.75f;
    private float lastTeapotDestroyedTime = -999f;

    [Header("Audio")]
    [SerializeField] private AudioClip teapotPlaceSFX;
    [SerializeField] private AudioSource audioSource;

    private Coroutine spawnRoutine;
    private GameObject currentTeapot;

    public GameObject CurrentTeapot => currentTeapot;

    public void SetPlayerOnLilypad(bool value)
    {
        playerOnLilypad = value;
        Debug.Log($"[LilyStool] {name} PlayerOnLilypad = {playerOnLilypad}");

        if (playerOnLilypad)
        {
            StartTeapotTimer();
        }
        else
        {
            StopTeapotTimer();
        }
    }

    public void NotifyTeapotDestroyed()
    {
        lastTeapotDestroyedTime = Time.time;
        currentTeapot = null;
        Debug.Log($"[LilyStool] {name} NotifyTeapotDestroyed at time {lastTeapotDestroyedTime}");
    }

    private void Update()
    {
        // Clear stale reference if destroyed elsewhere
        if (currentTeapot == null)
        {
            currentTeapot = null;
        }
    }

    private void StartTeapotTimer()
    {
        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnRoutine = StartCoroutine(TeapotSpawnDelay());
    }

    private void StopTeapotTimer()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator TeapotSpawnDelay()
    {
        Debug.Log($"[LilyStool] Starting teapot timer ({spawnDelay}s)");

        yield return new WaitForSeconds(spawnDelay);

        while (playerOnLilypad)
        {
            // Prevent instant respawn right after brewing/destroying
            if (Time.time - lastTeapotDestroyedTime < postBrewSpawnBlockTime)
            {
                yield return null;
                continue;
            }

            if (currentTeapot != null)
            {
                yield break;
            }

            if (IsTeacupPresent())
            {
                Debug.Log("[LilyStool] Teacup detected -> blocking teapot spawn");

                if (!retryWhileBlocked)
                    yield break;

                yield return new WaitForSeconds(retryDelay);
                continue;
            }

            SpawnTeapot();
            yield break;
        }
    }

    private bool IsTeacupPresent()
    {
        GameObject teacup = GameObject.FindGameObjectWithTag(teacupTag);
        return teacup != null;
    }

    private void SpawnTeapot()
    {
        if (teapotPrefab == null || teapotSpawnPoint == null)
        {
            Debug.LogWarning("[LilyStool] Missing teapotPrefab or spawn point");
            return;
        }

        if (currentTeapot != null)
        {
            Debug.Log("[LilyStool] Teapot already exists, skipping spawn");
            return;
        }

        currentTeapot = Instantiate(teapotPrefab, teapotSpawnPoint.position, Quaternion.identity);
        Debug.Log("[LilyStool] Teapot spawned automatically");

        PlayTeapotSFX();
    }

    public void DestroyCurrentTeapot()
    {
        if (currentTeapot != null)
        {
            Destroy(currentTeapot);
            currentTeapot = null;
            lastTeapotDestroyedTime = Time.time;
            Debug.Log("[LilyStool] Current teapot destroyed.");
        }
    }

    private void PlayTeapotSFX()
    {
        if (teapotPlaceSFX == null)
        {
            Debug.LogWarning("[LilyStool] No teapotPlaceSFX assigned");
            return;
        }

        if (audioSource != null)
        {
            audioSource.PlayOneShot(teapotPlaceSFX);
        }
        else
        {
            AudioSource.PlayClipAtPoint(teapotPlaceSFX, teapotSpawnPoint.position);
        }
    }
}