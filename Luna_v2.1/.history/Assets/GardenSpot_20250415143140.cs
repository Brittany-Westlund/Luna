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
        if (grassRenderer != null && active)
        {
            grassRenderer.color = Color.white;
        }
        // NO ELSE — do NOT reset color, let your scene default handle that
    }
}
