using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    public GameObject grassObject;
    public Transform plantingPoint;

    private GameObject plantedFlower;
    private SpriteRenderer grassRenderer;
    private Color originalColor;

    void Start()
    {
        if (grassObject != null)
        {
            grassRenderer = grassObject.GetComponent<SpriteRenderer>();
            if (grassRenderer != null)
            {
                originalColor = grassRenderer.color;
            }
        }

        if (plantingPoint == null)
        {
            Transform found = transform.Find("PlantingPoint");
            if (found != null)
            {
                plantingPoint = found;
            }
            else
            {
                Debug.LogWarning($"🌾 GardenSpot '{name}' is missing a planting point.");
            }
        }
    }

    public void SetHighlight(bool active)
    {
        if (grassRenderer != null)
        {
            grassRenderer.color = active ? Color.white : originalColor;
        }
    }

    public void SetPlantedFlower(GameObject flower)
    {
        plantedFlower = flower;
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
