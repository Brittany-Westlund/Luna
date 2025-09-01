using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    [Header("Held Flower Settings")]
    public Transform holdPoint;

    private GameObject heldFlower;
    private string heldFlowerType = "";
    private List<GardenSpot> nearbyGardens = new List<GardenSpot>();

    public string currentFlowerType => heldFlowerType;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            GardenSpot closest = FindClosestGarden();
            if (closest == null) return;

            GameObject planted = closest.GetPlantedFlower();

            if (HasFlower())
            {
                if (planted != null)
                {
                    Debug.Log("[🌱 PLANTING] Replacing existing flower.");
                    closest.RemovePlantedFlower();
                }
                PlantInGarden(closest);
            }
            else if (planted != null)
            {
                Debug.Log("[🤲 PICKUP] Picking up planted flower.");
                PickUpFlower(planted);
                closest.RemovePlantedFlower();
            }
        }
    }

    public bool HasFlower()
    {
        return heldFlower != null;
    }

    public GameObject GetHeldFlower()
    {
        return heldFlower;
    }

    public void DropFlower()
    {
        heldFlower = null;
        heldFlowerType = "";
    }

    public void PickUpFlowerInstance(GameObject flower)
    {
        PickUpFlower(flower);
    }

    private void PickUpFlower(GameObject flower)
    {
        if (flower == null) return;

        heldFlower = flower;
        heldFlowerType = flower.GetComponent<FlowerPickup>()?.flowerType ?? "Unknown";

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = true;

        Transform pivot = flower.transform.Find("pivot");
        Vector3 offset = pivot != null ? flower.transform.position - pivot.position : Vector3.zero;

        flower.transform.position = holdPoint.position + offset;
        flower.transform.SetParent(holdPoint);
        flower.GetComponent<Collider2D>().enabled = false;

        Debug.Log($"[🤲 Picked up flower: {flower.name}]");
    }

    private void PlantInGarden(GardenSpot garden)
    {
        if (!HasFlower()) return;

        Transform pivot = heldFlower.transform.Find("pivot");
        Transform plantingPoint = garden.transform.Find("PlantingPoint");

        if (pivot == null || plantingPoint == null)
        {
            Debug.LogWarning("Missing pivot or planting point.");
            return;
        }

        Vector3 offset = heldFlower.transform.position - pivot.position;
        heldFlower.transform.position = plantingPoint.position + offset;
        heldFlower.transform.SetParent(null);
        heldFlower.GetComponent<Collider2D>().enabled = true;

        var sprout = heldFlower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = false;

        garden.SetPlantedFlower(heldFlower);
        Debug.Log($"[🌱 PLANTED] {heldFlower.name} in {garden.name}");

        DropFlower();
    }

    private GardenSpot FindClosestGarden()
    {
        if (nearbyGardens.Count == 0) return null;

        GardenSpot closest = null;
        float shortest = float.MaxValue;

        foreach (var g in nearbyGardens)
        {
            float dist = Vector3.Distance(transform.position, g.transform.position);
            if (dist < shortest)
            {
                shortest = dist;
                closest = g;
            }
        }

        return closest;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            GardenSpot garden = other.GetComponent<GardenSpot>();
            if (garden != null && !nearbyGardens.Contains(garden))
            {
                nearbyGardens.Add(garden);
                garden.SetHighlight(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            GardenSpot garden = other.GetComponent<GardenSpot>();
            if (garden != null && nearbyGardens.Contains(garden))
            {
                nearbyGardens.Remove(garden);
                garden.SetHighlight(false);
            }
        }
    }
}