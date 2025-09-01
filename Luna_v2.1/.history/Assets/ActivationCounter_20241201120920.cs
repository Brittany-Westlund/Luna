using UnityEngine;
using PixelCrushers.DialogueSystem; // For Dialogue System variable access
using System.Collections;

public class ActivationCounter : MonoBehaviour
{
    [Header("Generic Activation Settings")]
    public string variableName = "ActivationCount";    // The Dialogue System variable to increment
    public bool incrementOnEnable = true;              // Whether to increment when SpriteRenderer is enabled
    public bool useSpriteRenderer = true;              // Set to true if activation depends on a SpriteRenderer
    public int incrementAmount = 1;                    // Amount to increment the variable by

    private SpriteRenderer spriteRenderer;
    private bool hasIncremented = false;               // Ensure it only increments once per enable

    void Start()
    {
        // Get the SpriteRenderer if needed
        if (useSpriteRenderer)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
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

        // Ensure variable name is valid
        if (string.IsNullOrEmpty(variableName))
        {
            yield break;
        }

        // Increment the specified Dialogue System variable by the incrementAmount
        int currentCount = DialogueLua.GetVariable(variableName).asInt;
        int newCount = currentCount + incrementAmount;
        DialogueLua.SetVariable(variableName, newCount);

    }

    // Reset the incremented flag, allowing the object to be reused if needed
    public void ResetIncrement()
    {
        hasIncremented = false;
    }
}
