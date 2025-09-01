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
        {
            grassRenderer.color = active ? Color.white : originalColor;
        }
    }

    public void Plant(GameObject flower, Vector3 position)
    {
        if (flower == null) return;

        // Set parent so the flower stays associated with this garden
        flower.transform.SetParent(transform);
        flower.transform.position = position;

        plantedFlower = flower;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = false;
            sprout.isPlanted = true;
        }

        var col = flower.GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
        }

        flower.SetActive(true);
    }

    public GameObject PickUp()
    {
        if (plantedFlower == null) return null;

        GameObject flower = plantedFlower;
        plantedFlower = null;

        flower.transform.SetParent(null);

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = true;
            sprout.isPlanted = false;
        }

        var col = flower.GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        return flower;
    }

    public GameObject GetPlantedFlower()
    {
        return plantedFlower;
    }
}
