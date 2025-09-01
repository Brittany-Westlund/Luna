using UnityEngine;
using System.Collections;

public class PresentSpore : MonoBehaviour
{
    public GameObject sporePrefab; // The prefab to instantiate as the spore
    public Transform attachPoint; // The point where the spore will attach to Luna
    public float slideSpeed = 1.5f; // Speed of the slide-in animation
    public float slideOffset = 0.1f; // Offset above the attach point to start the slide

    private GameObject activeSpore; // Reference to the currently instantiated spore
    private Coroutine slideCoroutine; // Reference to the slide coroutine
    private bool isSliding = false; // Tracks whether the spore is sliding into place

    public bool HasSporeAttached => activeSpore != null && !isSliding;

    public GameObject GetAttachedSpore()
    {
        return activeSpore;
    }

    public bool IsSliding => isSliding;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) // Press 'S' to toggle spore
        {
            if (HasSporeAttached)
            {
                DetachSpore();
            }
            else
            {
                CreateSpore();
            }
        }
    }

    public void CreateSpore()
    {
        if (sporePrefab == null || attachPoint == null)
        {
            Debug.LogError("Spore prefab or attach point is not assigned!");
            return;
        }

        if (activeSpore != null)
        {
            Debug.LogWarning("Spore already attached!");
            return;
        }

        // Instantiate the spore above the attach point
        Vector3 spawnPosition = attachPoint.position + Vector3.up * slideOffset;
        activeSpore = Instantiate(sporePrefab, spawnPosition, Quaternion.identity);

        // Start the slide-in animation
        slideCoroutine = StartCoroutine(SlideSporeIntoPlace());
    }

    public void DetachSpore()
    {
        if (activeSpore != null && !isSliding)
        {
            // Detach the spore
            StopSliding();
            activeSpore.transform.SetParent(null);
            Destroy(activeSpore);
            activeSpore = null;

            Debug.Log("Spore detached and removed.");
        }
    }

    IEnumerator SlideSporeIntoPlace()
    {
        isSliding = true;
        while (Vector3.Distance(activeSpore.transform.position, attachPoint.position) > 0.01f)
        {
            activeSpore.transform.position = Vector3.MoveTowards(
                activeSpore.transform.position,
                attachPoint.position,
                slideSpeed * Time.deltaTime
            );
            yield return null;
        }

        // Snap to position and parent the spore
        activeSpore.transform.position = attachPoint.position;
        activeSpore.transform.SetParent(attachPoint);
        isSliding = false;
        Debug.Log("Spore has been attached to the player.");
    }

    private void StopSliding()
    {
        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
            isSliding = false;
        }
    }
}
