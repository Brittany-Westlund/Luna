using UnityEngine;
using PixelCrushers.DialogueSystem; // For Dialogue System variable access

public class ActivationCounter : MonoBehaviour
{
    [Header("Activation Settings")]
    public string activationType = "LanternLitCount"; // Dialogue System variable to increment
    public bool incrementOnEnable = true;             // Whether to increment when SpriteRenderer is enabled
    public bool useSpriteRenderer = true;             // Set to true if activation depends on a SpriteRenderer

    private SpriteRenderer spriteRenderer;
    private bool hasIncremented = false;              // Track if this object has already incremented the counter

    void Start()
    {
        // Get the SpriteRenderer component if we're using it for activation checks
        if (useSpriteRenderer)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"SpriteRenderer not found on {gameObject.name}");
            }
        }
    }

    void Update()
    {
        // Check if the SpriteRenderer is enabled (or if activation isn't dependent on the SpriteRenderer)
        if (!hasIncremented && incrementOnEnable && (!useSpriteRenderer || (spriteRenderer != null && spriteRenderer.enabled)))
        {
            IncrementActivationCount();
            hasIncremented = true; // Ensure it only increments once
        }
    }

    private void IncrementActivationCount()
    {
        // Increment the Dialogue System variable for the activation type
        int currentCount = DialogueLua.GetVariable(activationType).asInt;
        DialogueLua.SetVariable(activationType, currentCount + 1);
        Debug.Log($"Incremented {activationType}. New {activationType} value: {currentCount + 1}");
    }

    // Optional: Call this method to allow the lantern to increment the count again
    public void ResetIncrement()
    {
        hasIncremented = false;
    }
}
