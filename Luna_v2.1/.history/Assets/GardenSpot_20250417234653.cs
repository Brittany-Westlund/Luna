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

    public Transform GetPlantingPoint()
    {
        return transform.Find("PlantingPoint") ?? transform;
    }

    public void SetHighlight(bool active)
    {
        if (grassRenderer != null)
            grassRenderer.color = active ? Color.white : originalColor;
    }
}
