// Checkpoint.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    public string checkpointKey; // e.g., "river"

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && ThoughtBubbleManager.Instance)
            ThoughtBubbleManager.Instance.SetCheckpointKey(checkpointKey);
    }
}
