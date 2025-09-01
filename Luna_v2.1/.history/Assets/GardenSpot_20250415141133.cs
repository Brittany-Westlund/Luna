using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    [Header("References")]
    public GameObject grassObject;

    [HideInInspector] public bool isPlanted = false;

    private SpriteRenderer grassRenderer;

    private void Start()
    {
        if (grassObject != null)
        {
            grassRenderer = grassObject.GetComponent<SpriteRenderer>();
        }
        else
        {
            Debug.LogWarning("🌱 GardenSpot is missing its Grass reference.");
        }
    }

    public void SetHighlight(bool active)
    {
        if (grassRenderer != null)
        {
            grassRenderer.color = active ? Color.white : new Color(0.5f, 0.5f, 0.5f); // dark gray when inactive
        }
    }
}
