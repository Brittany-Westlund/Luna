using UnityEngine;
using System.Collections;

public class Aid : MonoBehaviour
{
    public float aidDistance = 3f; // Distance the spore moves when aiding
    public float moveSpeed = 5f; // Speed of the spore's movement

    private PresentSpore presentSpore; // Reference to the PresentSpore script
    private Transform lunaTransform; // Reference to Luna's transform

    void Start()
    {
        presentSpore = GetComponent<PresentSpore>();
        lunaTransform = transform; // Assuming this script is attached to Luna
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && presentSpore.HasSporeAttached && !presentSpore.IsSliding)
        {
            GameObject attachedSpore = presentSpore.GetAttachedSpore();
            if (attachedSpore != null)
            {
                // Determine the facing direction based on Luna's local scale
                Vector3 direction = lunaTransform.localScale.x > 0 ? Vector3.right : Vector3.left;

                Debug.Log($"Aid triggered. Moving spore in direction: {direction}");

                // Start moving the spore and handle destruction
                StartCoroutine(SlideSporeForwardAndDestroy(attachedSpore, direction));

                // Reset the state of PresentSpore
                presentSpore.ResetSporeState();
            }
        }
    }

    private IEnumerator SlideSporeForwardAndDestroy(GameObject spore, Vector3 direction)
    {
        if (spore == null)
        {
            Debug.LogError("No spore found to move!");
            yield break;
        }

        // Calculate the target position
        Vector3 startPosition = spore.transform.position;
        Vector3 targetPosition = startPosition + direction * aidDistance;

        Debug.Log($"Spore starting at {startPosition}, moving to {targetPosition}");

        // Ensure spore is visible
        Renderer sporeRenderer = spore.GetComponent<Renderer>();
        if (sporeRenderer != null)
        {
            sporeRenderer.enabled = true;
            Debug.Log("Spore renderer is enabled.");
        }
        else
        {
            Debug.LogWarning("Spore has no Renderer component.");
        }

        // Move the spore toward the target position
        while (Vector3.Distance(spore.transform.position, targetPosition) > 0.01f)
        {
            spore.transform.position = Vector3.MoveTowards(
                spore.transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null; // Wait until the next frame
        }

        Debug.Log("Spore reached the target and will be destroyed.");

        // Destroy the spore after reaching the target position
        Destroy(spore);
    }
}
