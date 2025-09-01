// GardenSpot.cs
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GardenSpot : MonoBehaviour
{
    [Header("Where to place the flower")]
    public Transform plantingPoint;

    private GameObject plantedFlower;
    private SpriteRenderer highlightRenderer;

    void Awake()
    {
        var h = transform.Find("Highlight");
        if (h != null) highlightRenderer = h.GetComponent<SpriteRenderer>();
    }

    public void SetHighlight(bool on)
    {
        if (highlightRenderer != null)
            highlightRenderer.enabled = on;
    }

    public void SetPlantedFlower(GameObject flower)
    {
        plantedFlower = flower;
    }

    public GameObject GetPlantedFlower()
    {
        return plantedFlower;
    }

    public void ClearPlantedFlower()
    {
        plantedFlower = null;
    }
}
