using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    public GameObject grassObject;

    private GameObject plantedFlower;
    private SpriteRenderer grassRenderer;
    private Color originalColor;

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

    public bool HasPlantedFlower => plantedFlower != null;

    public Transform GetPlantingPoint()
    {
        return transform.Find("PlantingPoint") ?? transform;
    }

    public void Plant(GameObject flower, Vector3 position)
    {
        plantedFlower = flower;
        flower.transform.SetParent(null);
        flower.transform.position = position;

        var col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = false;

        flower.SetActive(true);
    }

    public GameObject PickUp()
    {
        if (plantedFlower == null) return null;

        GameObject flower = plantedFlower;
        plantedFlower = null;

        var col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        flower.transform.SetParent(null);
        return flower;
    }

    public void ClearPlantedFlowerReference()
    {
        plantedFlower = null;
    }
}
