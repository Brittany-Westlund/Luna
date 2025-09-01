using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    public GameObject grassObject;
    public Transform plantingPoint;

    private SpriteRenderer grassRenderer;
    private Color originalColor;
    private GameObject _plantedFlower;

    private void Start()
    {
        if (grassObject != null)
        {
            grassRenderer = grassObject.GetComponent<SpriteRenderer>();
            originalColor = grassRenderer.color;
        }
    }

    public void SetHighlight(bool active)
    {
        if (grassRenderer == null) return;

        grassRenderer.color = active ? Color.white : originalColor;
    }

    public bool HasPlantedFlower => _plantedFlower != null;

    public GameObject PickUp()
    {
        GameObject picked = _plantedFlower;
        _plantedFlower = null;
        return picked;
    }

    public void Plant(GameObject flower, Vector3 position)
    {
        if (flower == null) return;

        Transform pivot = flower.transform.Find("pivot");
        Vector3 offset = pivot != null ? flower.transform.position - pivot.position : Vector3.zero;

        flower.transform.position = position + offset;
        flower.transform.SetParent(null);

        Collider2D col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = false;

        _plantedFlower = flower;
    }

    public Transform GetPlantingPoint()
    {
        return plantingPoint != null ? plantingPoint : transform;
    }
}
