using UnityEngine;
using MoreMountains.Tools;
using System.Collections;

public class PresentSpore : MonoBehaviour
{
    public GameObject sporePrefab; // The prefab to instantiate as the spore
    public Transform attachPoint; // The point where the spore will attach to Luna
    public MMProgressBar healthBar; // Reference to the MMProgressBar
    public int maxHealth = 100; // Maximum health
    public int health = 100; // Current health
    public float slideSpeed = 1.5f; // Speed of the slide-in animation
    public float slideOffset = 0.1f; // Offset above the attach point to start the slide

    private GameObject activeSpore; // Reference to the currently instantiated spore
    private Coroutine slideCoroutine; // Reference to the slide coroutine

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) // Toggle spore on/off with 'S'
        {
            if (activeSpore == null)
            {
                CreateSpore();
                health -= 1; // Decrease health when the spore is attached
            }
            else
            {
                DetachSpore();
                health += 1; // Restore health when the spore is detached
            }
            UpdateHealthBar();
        }
    }

    void CreateSpore()
    {
        if (sporePrefab == null || attachPoint == null)
        {
            Debug.LogError("Spore prefab or attach point is not assigned!");
            return;
        }

        // Instantiate the spore above the attach point
        Vector3 spawnPosition = attachPoint.position + Vector3.up * slideOffset;
        activeSpore = Instantiate(sporePrefab, spawnPosition, Quaternion.identity);

        // Start the slide-in animation
        slideCoroutine = StartCoroutine(SlideSporeIntoPlace());
        Debug.Log($"Spore {activeSpore.name} created and sliding into place.");
    }

    void DetachSpore()
    {
        if (activeSpore != null)
        {
            // Stop the slide animation if it's still running
            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
            }

            activeSpore.transform.SetParent(null); // Detach the spore
            Destroy(activeSpore); // Destroy the spore
            activeSpore = null;

            Debug.Log("Spore detached and removed.");
        }
    }

    IEnumerator SlideSporeIntoPlace()
    {
        // Continue sliding until the spore reaches the attach point
        while (Vector3.Distance(activeSpore.transform.position, attachPoint.position) > 0.01f)
        {
            activeSpore.transform.position = Vector3.MoveTowards(
                activeSpore.transform.position,
                attachPoint.position,
                slideSpeed * Time.deltaTime
            );
            yield return null; // Wait until the next frame
        }

        // Snap to position and parent the spore
        activeSpore.transform.position = attachPoint.position;
        activeSpore.transform.SetParent(attachPoint);
        Debug.Log("Spore has been attached to the player.");
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.UpdateBar(health, 0, maxHealth); // Update the progress bar
        }
        else
        {
            Debug.LogWarning("Health bar is not assigned!");
        }
    }
}
