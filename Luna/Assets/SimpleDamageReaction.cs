using UnityEngine;

public class SimpleDamageReaction : MonoBehaviour
{
    public GameObject reactionBubblePrefab; // Reference to the reaction bubble prefab
    public Vector3 bubbleOffset = new Vector3(0, 1.5f, 0); // Offset to position the bubble above Luna's head
    public float displayDuration = 1.5f; // Duration the bubble stays visible

    private bool hasReacted = false; // Flag to ensure the reaction happens only once

    // Method to show the damage reaction
    public void ShowDamageReaction()
    {
        // Check if the reaction has already been shown
        if (!hasReacted)
        {
            // Instantiate the reaction bubble at Luna's position with an offset
            GameObject bubble = Instantiate(reactionBubblePrefab, transform.position + bubbleOffset, Quaternion.identity);

            // Destroy the bubble after a set duration
            Destroy(bubble, displayDuration);

            // Set the flag to true to prevent further reactions
            hasReacted = true;
        }
    }

    // Optional method to reset the reaction flag if needed
    public void ResetReaction()
    {
        hasReacted = false;
    }
}
