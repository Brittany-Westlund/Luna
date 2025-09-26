// CheckpointBubbleSetter.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CheckpointBubbleSetter : MonoBehaviour
{
    [Tooltip("Which bubble index becomes the new default after touching this checkpoint")]
    public int bubbleIndex = 0;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var bubbles = other.GetComponentInChildren<SimpleThoughtBubbles>(true);
        if (bubbles == null) return;

        // Make checkpoint the new default going forward
        bubbles.defaultIndex = Mathf.Clamp(bubbleIndex, 0, (bubbles.bubbles?.Length ?? 1) - 1);
        bubbles.SetIndex(bubbles.defaultIndex);
    }
}
