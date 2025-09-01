// GardenSpot.cs
using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    [Tooltip("Child Transform where the flower should snap to.")]
    public Transform plantingPoint;
    [Tooltip("Optional outline/highlight object.")]
    public GameObject highlightObject;

    // Which flower is currently planted here
    private GameObject plantedFlower;

    void Awake()
    {
        // If you manually placed a flower as a child in the editor,
        // register it now so GetPlantedFlower() returns it.
        for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i).gameObject;
            if (c.GetComponent<SproutAndLightManager>() != null)
            {
                plantedFlower = c;
                var mgr = c.GetComponent<SproutAndLightManager>();
                mgr.isPlanted = true;
                break;
            }
        }
    }

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
