using UnityEngine;
using PixelCrushers.DialogueSystem; // For Dialogue System variable access
using System.Collections;

public class ActivationCounter : MonoBehaviour
{
    [Header("Generic Activation Settings")]
    public string variableName = "ActivationCount";    // The Dialogue System variable to increment
    public bool incrementOnEnable = true;              // Whether to increment when SpriteRenderer is enabled
    public bool useSpriteRenderer = true;              // Set to true if activation depends on a SpriteRenderer

    private SpriteRenderer spriteRenderer;
    private bool hasIncremented = false;               // Ensure it only increments once per enable

    void Start()
    {
        // Get the SpriteRenderer if needed
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

        // Wait one frame to avoid duplicate increments
        yield return null;

        // Increment the specified Dialogue System variable
        int currentCount = DialogueLua.GetVariable(variableName).asInt;
        DialogueLua.SetVariable(variableName, currentCount + 1);
        Debug.Log($"Incremented {variableName}. New {variableName} value: {currentCount + 1}");
    }

    // Reset the incremented flag, allowing the object to be reused if needed
    public void ResetIncrement()
    {
        hasIncremented = false;
    }
}
