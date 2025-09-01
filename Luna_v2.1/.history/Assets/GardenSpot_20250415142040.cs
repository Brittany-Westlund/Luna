using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    public GameObject grassObject;

    private SpriteRenderer grassRenderer;

    private void Start()
    {
        if (grassObject != null)
            grassRenderer = grassObject.GetComponent<SpriteRenderer>();
    }

    public void SetHighlight(bool active)
    {
        if (grassRenderer != null)
        {
            grassRenderer.color = active ? Color.white : new Color(0.5f, 0.5f, 0.5f);
        }
    }
}
