using UnityEngine;
using System.Collections;

public class PresentSpore : MonoBehaviour
{
    public GameObject sporePrefab; // The prefab to instantiate as the spore
    public Transform attachPoint; // The point where the spore will attach to Luna
    public float slideSpeed = 5f; // Speed of the slide-in animation
    public float slideOffset = 2f; // Offset above the attach point to start the slide

    private GameObject activeSpore; // Reference to the currently instantiated spore
    private Coroutine slideCoroutine; // Reference to the slide coroutine

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) // Press 'S' to create the spore
        {
            if (activeSpore == null) // Only instantiate if no spore exists
            {
                CreateSpore();
            }
            else
            {
                Debug.Log("A spore is already present!");
            }
        }

        if (Input.GetKeyDown(KeyCode.D)) // Press 'D' to detach/remove the spore
        {
            DetachSpore();
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
}
