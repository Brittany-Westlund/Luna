using UnityEngine;

public class TeacupHighlight : MonoBehaviour
{
    public SpriteRenderer highlightRenderer; // Drag in Inspector

    public void Highlight()
    {
        if (highlightRenderer != null) highlightRenderer.enabled = true;
    }

    public void RemoveHighlight()
    {
        if (highlightRenderer != null) highlightRenderer.enabled = false;
    }
}
