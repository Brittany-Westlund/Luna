using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    public GameObject grassObject;

    private SpriteRenderer grassRenderer;
    private Color originalColor;

    private void Start()
    {
        if (grassObject != null)
        {
            grassRenderer = grassObject.GetComponent<SpriteRenderer>();
            originalColor = grassRenderer.color; // 🧠 Save the styled color
        }
    }

    public void SetHighlight(bool active)
    {
        if (grassRenderer == null) return;

        grassRenderer.color = active ? Color.white : originalColor;
    }
}
