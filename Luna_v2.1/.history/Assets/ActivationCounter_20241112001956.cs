using UnityEngine;

public class ActivationCounter : MonoBehaviour
{
    [Header("Activation Settings")]
    public string activationType = "LanternLitCount"; // Type of activation to match with InteractableObject
    public bool incrementOnEnable = true;             // Whether to increment the count when the object is enabled
    public KeyCode activationKey = KeyCode.None;      // Optional key to manually trigger the activation (set to None if unused)

    private InteractableObject interactableObject;    // Reference to the InteractableObject script

    void Start()
    {
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
        // Increment the count if incrementOnEnable is true
        if (incrementOnEnable)
        {
            IncrementActivationCount();
        }
    }

    void Update()
    {
        // Check if the activation key is pressed and trigger increment if so
        if (activationKey != KeyCode.None && Input.GetKeyDown(activationKey))
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
