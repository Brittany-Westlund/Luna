using UnityEngine;
using PixelCrushers.DialogueSystem; // For Dialogue System variable access
using System.Collections;

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
        // Get the SpriteRenderer component if needed
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
        // Check if SpriteRenderer is enabled, or just increment if useSpriteRenderer is false
        if (!hasIncremented && incrementOnEnable && (!useSpriteRenderer || (spriteRenderer != null && spriteRenderer.enabled)))
        {
            StartCoroutine(IncrementOnce());
        }
    }

    private IEnumerator IncrementOnce()
    {
        hasIncremented = true; // Immediately set to prevent repeated calls

        // Wait one frame to ensure Update doesn’t trigger multiple times
        yield return null;

        // Increment the specified Dialogue System variable
        int currentCount = DialogueLua.GetVariable(activationType).asInt;
        DialogueLua.SetVariable(activationType, currentCount + 1);
        Debug.Log($"Incremented {activationType}. New {activationType} value: {currentCount + 1}");
    }

    // Optional: Reset the incremented flag if needed for repeated use
    public void ResetIncrement()
    {
        hasIncremented = false;
    }
}
