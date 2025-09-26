// ThoughtBubbleRegistry.cs
using UnityEngine;

public class ThoughtBubbleRegistry : MonoBehaviour
{
    [System.Serializable]
    public struct Entry
    {
        public string key;          // e.g., "deer", "river", "memory"
        public Transform bubbleRoot; // the child under ParentBubble
    }

    [Header("All bubble variants under ParentBubble")]
    public Transform parentBubble;     // assign your "ParentBubble"
    public Entry[] bubbles;

    public Transform Get(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        for (int i = 0; i < bubbles.Length; i++)
            if (bubbles[i].key == key) return bubbles[i].bubbleRoot;
        return null;
    }

    // Hide all children (call on enable/disable for safety)
    public void HideAll()
    {
        if (!parentBubble) return;
        // Only touch children of ParentBubble, never siblings or parents
        foreach (Transform t in parentBubble)
        {
            var srs = t.GetComponentsInChildren<SpriteRenderer>(true);
            for (int i = 0; i < srs.Length; i++)
                srs[i].enabled = false;
        }
    }

    // Show a specific bubble (enables its SpriteRenderers only)
    public void Show(Transform root, bool visible)
    {
        if (!root) return;
        foreach (var sr in root.GetComponentsInChildren<SpriteRenderer>(true))
            sr.enabled = visible;
    }
}
