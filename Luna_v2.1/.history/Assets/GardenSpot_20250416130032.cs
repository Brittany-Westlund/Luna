using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    public Transform plantingPoint;
    public GameObject grassObject;

    private GameObject plantedFlower;
    private SpriteRenderer grassRenderer;
    private Color originalColor;

    public bool HasPlantedFlower => plantedFlower != null;

    private void Start()
    {
        if (grassObject != null)
        {
            grassRenderer = grassObject.GetComponent<SpriteRenderer>();
            if (grassRenderer != null)
                originalColor = grassRenderer.color;
        }
    }

    public void SetHighlight(bool active)
    {
        if (grassRenderer != null)
        {
            grassRenderer.color = active ? Color.white : originalColor;
        }
    }

    public Transform GetPlantingPoint() => plantingPoint;

    public void Plant(GameObject flower, Vector3 position)
    {
        plantedFlower = flower;

        flower.transform.SetParent(null);
        flower.transform.position = position;
        flower.GetComponent<Collider2D>().enabled = true;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = false;

        flower.SetActive(true);
    }

    public GameObject PickUp()
    {
        if (plantedFlower == null) return null;

        GameObject flower = plantedFlower;
        plantedFlower = null;
        return flower;
    }
}
