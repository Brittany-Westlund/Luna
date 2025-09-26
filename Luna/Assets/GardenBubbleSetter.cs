// GardenBubbleSetter.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GardenBubbleSetter : MonoBehaviour
{
    [Tooltip("Which bubble index to use while inside this garden")]
    public int bubbleIndex = 0;

    private int prevIndex = -1;
    private SimpleThoughtBubbles bubbles;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (bubbles == null)
            bubbles = other.GetComponentInChildren<SimpleThoughtBubbles>(true);
        if (bubbles == null) return;

        prevIndex = bubbles.CurrentIndex;
        bubbles.SetIndex(bubbleIndex);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (bubbles == null) return;

        // restore whatever was active before entering this garden
        if (prevIndex >= 0) bubbles.SetIndex(prevIndex);
        prevIndex = -1;
    }
}
