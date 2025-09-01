using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    [Tooltip("Child Transform where the flower should be placed.")]
    public Transform plantingPoint;
    public GameObject highlightObject;

    private GameObject plantedFlower;

    public void SetHighlight(bool on)
    {
        if (highlightObject != null)
            highlightObject.SetActive(on);
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
        return plantingPoint;
    }
}
