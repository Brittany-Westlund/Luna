// GardenThoughtZone.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GardenThoughtZone : MonoBehaviour
{
    public string gardenKey; // e.g., "deer"

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && ThoughtBubbleManager.Instance)
            ThoughtBubbleManager.Instance.SetLocalKey(gardenKey);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && ThoughtBubbleManager.Instance)
            ThoughtBubbleManager.Instance.ClearLocalKey();
    }
}
