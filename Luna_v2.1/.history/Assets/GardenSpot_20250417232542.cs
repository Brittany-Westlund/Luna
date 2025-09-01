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
        if (flower == null) return;

        plantedFlower = flower;
        flower.transform.SetParent(null);
        flower.transform.position = position;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isPlanted = true;
            sprout.isHeld = false;
        }

        Collider2D col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        flower.SetActive(true);
    }

    public GameObject PickUp()
    {
        if (plantedFlower == null) return null;

        GameObject temp = plantedFlower;
        plantedFlower = null;

        var sprout = temp.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isPlanted = false;
            sprout.isHeld = true;
        }

        Collider2D col = temp.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        return temp;
    }

    public bool HasPlantedFlower => plantedFlower != null;
}
