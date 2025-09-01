using UnityEngine;
using System.Collections;

public class Aid : MonoBehaviour
{
    public float aidDistance = 3f; // Distance the spore moves when aiding
    public float moveSpeed = 5f; // Speed of the spore's movement

    private PresentSpore presentSpore; // Reference to the PresentSpore script
    private Transform sporeHoldPoint; // Reference to the spore's attach point

    void Start()
    {
        presentSpore = GetComponent<PresentSpore>();
        sporeHoldPoint = presentSpore.attachPoint; // Reference to the spore's parent point
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && presentSpore.HasSporeAttached && !presentSpore.IsSliding)
        {
            GameObject attachedSpore = presentSpore.GetAttachedSpore();
            if (attachedSpore != null)
            {
                // Determine the direction based on Luna's sprite renderer flip state
                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                Vector3 direction = spriteRenderer.flipX ? Vector3.left : Vector3.right;

                Debug.Log($"Aid triggered. Moving spore in direction: {direction}");

                // Start moving the spore and handle destruction
                StartCoroutine(SlideSporeForwardAndDestroy(attachedSpore, direction));
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

        // Detach from the parent to move independently
        spore.transform.SetParent(null);

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
        if (spore != null)
        {
            Destroy(spore);
        }

        // Reset the state of PresentSpore only after movement is complete
        presentSpore.ResetSporeState();
    }
}
