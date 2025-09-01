using UnityEngine;
using PixelCrushers.DialogueSystem; // Required for accessing Dialogue System variables

public class ActivationCounter : MonoBehaviour
{
    [Header("Activation Settings")]
    public string activationType = "LanternLitCount"; // The Dialogue System variable to increment
    public bool incrementOnEnable = true;             // Whether to increment when SpriteRenderer is enabled
    public bool useSpriteRenderer = true;             // Set to true if activation depends on a SpriteRenderer

    private SpriteRenderer spriteRenderer;
    private bool hasIncremented = false;              // Track if this object has already incremented the counter

    void Start()
    {
        // Find the SpriteRenderer component if needed
        if (useSpriteRenderer)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning("SpriteRenderer not found on " + gameObject.name);
            }
        }
    }

    void OnEnable()
    {
        // Check if SpriteRenderer is enabled, or just increment if useSpriteRenderer is false
        if (!hasIncremented && incrementOnEnable && (!useSpriteRenderer || (spriteRenderer != null && spriteRenderer.enabled)))
        {
            IncrementActivationCount();
            hasIncremented = true; // Ensure it only increments once
        }
    }

    private void IncrementActivationCount()
    {
        // Increment the specified Dialogue System variable
        int currentCount = DialogueLua.GetVariable(activationType).asInt;
        DialogueLua.SetVariable(activationType, currentCount + 1);
        Debug.Log($"Incremented {activationType} count. New {activationType} value: {currentCount + 1}");
    }

    // Optional: Reset the incremented flag if needed for repeated use
    public void ResetIncrement()
    {
        hasIncremented = false;
    }
}
