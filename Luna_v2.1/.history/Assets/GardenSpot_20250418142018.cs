// GardenSpot.cs
using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    [Tooltip("Child Transform where the flower will snap to.")]
    public Transform plantingPoint;

    [Tooltip("Object to toggle for highlight (e.g. an outline).")]
    public GameObject highlightObject;

    // The one flower this spot currently “owns”
    private GameObject plantedFlower;

    void Awake()
    {
        // If you pre‐placed a flower under this spot in the Editor,
        // pick it up here so our record is accurate.
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.GetComponent<SproutAndLightManager>() != null)
            {
                plantedFlower = child;
                var spr = child.GetComponent<SproutAndLightManager>();
                spr.isPlanted = true;
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
