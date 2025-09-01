using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    public GameObject grassObject;
    public bool assignStartingFlower = false;

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

        if (assignStartingFlower && plantedFlower == null)
        {
            Transform possible = transform.Find("PlantedFlower");
            if (possible != null)
            {
                Plant(possible.gameObject, GetPlantingPoint().position);
            }
        }
    }

    public bool HasPlantedFlower => plantedFlower != null;

    public Transform GetPlantingPoint()
    {
        return transform.Find("PlantingPoint") ?? transform;
    }

    public void SetHighlight(bool active)
    {
        if (grassRenderer != null)
            grassRenderer.color = active ? Color.white : originalColor;
    }

    public void Plant(GameObject flower, Vector3 position)
    {
        plantedFlower = flower;
        flower.transform.position = position;
        flower.transform.SetParent(transform); // ✅ Keep it childed for tracking

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = false;
            sprout.isPlanted = true;
        }

        var col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
        flower.SetActive(true);
    }

    public GameObject PickUp()
    {
        if (plantedFlower == null) return null;

        GameObject flower = plantedFlower;
        plantedFlower = null;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isPlanted = false;
            sprout.isHeld = true;
        }

        var col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        flower.transform.SetParent(null);
        return flower;
    }
}
