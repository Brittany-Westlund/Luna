using UnityEngine;

public class ActivationCounter : MonoBehaviour
{
    [Header("Activation Settings")]
    public string activationType = "LanternLitCount"; // Type of activation to match with InteractableObject
    public bool incrementOnEnable = true;             // Increment count when Sprite Renderer is enabled

    private SpriteRenderer spriteRenderer;
    private InteractableObject interactableObject;    // Reference to the InteractableObject script

    void Start()
    {
        // Find the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Find the InteractableObject that tracks the specified activationType
        interactableObject = FindObjectOfType<InteractableObject>();

        // Check if the interactableObject is set up for this activation type
        if (interactableObject == null || interactableObject.activationType != activationType)
        {
            Debug.LogWarning("InteractableObject not found or does not match activationType: " + activationType);
        }
    }

    void OnEnable()
    {
        // Check if the Sprite Renderer is enabled and increment if necessary
        if (incrementOnEnable && spriteRenderer != null && spriteRenderer.enabled)
        {
            IncrementActivationCount();
        }
    }

    private void IncrementActivationCount()
    {
        // Check if interactableObject is properly set and matches activation type
        if (interactableObject != null && interactableObject.activationType == activationType)
        {
            interactableObject.IncrementActivationCount();
            Debug.Log($"Incremented {activationType} count for {gameObject.name}");
        }
        else
        {
            Debug.LogWarning("InteractableObject not configured for this activation type.");
        }
    }
}
