// GardenSpot.cs
using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    [Tooltip("Child Transform where the flower should be placed.")]
    public Transform plantingPoint;

    [Tooltip("Optional: an object to enable/disable for highlight feedback.")]
    public GameObject highlightObject;

    // The one flower this spot currently holds (pre‑placed or planted)
    private GameObject plantedFlower;

    void Awake()
    {
        // Scan for any pre‑placed child with a SproutAndLightManager
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.GetComponent<SproutAndLightManager>() != null)
            {
                plantedFlower = child;
                // Mark it as planted so it glows, etc.
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
