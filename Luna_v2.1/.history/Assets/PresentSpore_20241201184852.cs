using UnityEngine;
using System.Collections;

public class PresentSpore : MonoBehaviour
{
    public GameObject sporePrefab; // The prefab to instantiate as the spore
    public Transform attachPoint; // The point where the spore will attach to Luna
    public float slideSpeed = 1.5f; // Speed of the slide animation
    public float slideOffset = 0.1f; // Offset above the attach point to start the slide

    private GameObject activeSpore; // Reference to the currently instantiated spore
    private Coroutine slideCoroutine; // Reference to the slide coroutine

    private bool _isSliding; // Internal field to track sliding state

    public bool IsSliding
    {
        get => _isSliding;
        set => _isSliding = value; // Allows external scripts to modify the sliding state
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) // Press 'S' to create/detach the spore
        {
            if (activeSpore == null) // No spore exists, create one
            {
                CreateSpore();
            }
            else if (!IsSliding) // Detach if not currently sliding
            {
                DetachSpore();
            }
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

            // Start the slide-out animation
            slideCoroutine = StartCoroutine(SlideSporeOutAndDestroy());
            Debug.Log("Spore sliding out and detaching.");
        }
    }

    IEnumerator SlideSporeIntoPlace()
    {
        IsSliding = true;

        // Slide the spore into the attach point
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
        IsSliding = false;

        Debug.Log("Spore has been attached to the player.");
    }

    IEnumerator SlideSporeOutAndDestroy()
    {
        IsSliding = true;

        // Slide the spore upward (reverse of attach animation)
        Vector3 targetPosition = attachPoint.position + Vector3.up * slideOffset;
        while (Vector3.Distance(activeSpore.transform.position, targetPosition) > 0.01f)
        {
            activeSpore.transform.position = Vector3.MoveTowards(
                activeSpore.transform.position, 
                targetPosition, 
                slideSpeed * Time.deltaTime
            );
            yield return null; // Wait until the next frame
        }

        Destroy(activeSpore); // Destroy the spore after it has slid out
        activeSpore = null;
        IsSliding = false;

        Debug.Log("Spore detached and removed.");
    }
}
