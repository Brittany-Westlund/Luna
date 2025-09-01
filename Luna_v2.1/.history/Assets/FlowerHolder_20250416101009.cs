using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    [Header("Held Flower Settings")]
    public Transform holdPoint;

    private GameObject heldFlower;
    private string heldFlowerType = "";
    private List<GardenSpot> nearbyGardens = new List<GardenSpot>();

    public bool HasFlower => heldFlower != null;
    public GameObject HeldFlower => heldFlower;
    public string CurrentFlowerType => heldFlowerType;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            GardenSpot closest = FindClosestGarden();
            if (closest == null) return;

            if (HasFlower)
            {
                if (closest.HasPlantedFlower())
                {
                    GameObject removed = closest.PickUp();
                    if (removed != null) Destroy(removed);
                }
                PlantInGarden(closest);
            }
            else if (closest.HasPlantedFlower())
            {
                GameObject flower = closest.PickUp();
                if (flower != null) PickUpFlower(flower);
            }
        }
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
        heldFlower = flower;
        heldFlowerType = flower.GetComponent<FlowerPickup>()?.flowerType ?? "Unknown";

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = true;

        Transform pivot = flower.transform.Find("pivot");
        Vector3 offset = pivot != null ? flower.transform.position - pivot.position : Vector3.zero;

        flower.transform.position = holdPoint.position + offset;
        flower.transform.SetParent(holdPoint);
        flower.GetComponent<Collider2D>().enabled = false;
    }

    private void PlantInGarden(GardenSpot garden)
    {
        if (!HasFlower) return;

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

        garden.Plant(heldFlower);
        DropFlower();
    }

    private GardenSpot FindClosestGarden()
    {
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