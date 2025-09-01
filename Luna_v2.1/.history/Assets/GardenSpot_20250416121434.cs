using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    public GameObject grassObject;
    private SpriteRenderer grassRenderer;
    private Color originalColor;
    private GameObject plantedFlower;

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

    public void Plant(GameObject flower)
    {
        plantedFlower = flower;
    }

    public bool HasPlantedFlower()
    {
        return plantedFlower != null;
    }

    public GameObject PickUp()
    {
        GameObject flower = plantedFlower;
        plantedFlower = null;
        return flower;
    }

    public GameObject GetPlantedFlower()
    {
        return plantedFlower;
    }

    public void RemovePlantedFlower()
    {
        plantedFlower = null;
    }
}
