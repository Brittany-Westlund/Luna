using UnityEngine;
using PixelCrushers.DialogueSystem;

public class ActivationCounter : MonoBehaviour
{
    [Header("Activation Settings")]
    public string activationType = "LanternLitCount"; // Dialogue System variable to increment
    public bool incrementOnEnable = true;             // Increment when SpriteRenderer is enabled
    public bool useSpriteRenderer = true;             // Activation depends on SpriteRenderer

    private SpriteRenderer spriteRenderer;
    private bool hasIncremented = false;              // Track if already incremented

    void Start()
    {
        // Get the SpriteRenderer if necessary
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
        // Check if SpriteRenderer is enabled and increment once
        if (!hasIncremented && incrementOnEnable && (!useSpriteRenderer || (spriteRenderer != null && spriteRenderer.enabled)))
        {
            IncrementActivationCount();
        }
    }

    private void IncrementActivationCount()
    {
        hasIncremented = true; // Only increment once
        int currentCount = DialogueLua.GetVariable(activationType).asInt;
        DialogueLua.SetVariable(activationType, currentCount + 1);
        Debug.Log($"Incremented {activationType}. New {activationType} value: {currentCount + 1}");
    }

    // Optional: Reset increment if needed
    public void ResetIncrement()
    {
        hasIncremented = false;
    }
}
