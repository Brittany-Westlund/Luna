using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    [Header("Hold Settings")]
    public Transform holdPoint;

    private GameObject heldFlower;
    private string heldFlowerType = "";
    private List<GardenSpot> nearbyGardens = new List<GardenSpot>();

    public bool HasFlower => heldFlower != null;
    public string CurrentFlowerType => heldFlowerType;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            GardenSpot closest = FindClosestGarden();
            if (closest == null) return;

            if (HasFlower)
            {
                var sprout = heldFlower.GetComponent<SproutAndLightManager>();
                if (sprout != null && !sprout.isPlanted)
                {
                    // Replace existing flower if needed
                    if (closest.HasPlantedFlower)
                    {
                        closest.PickUp(); // No need to store returned value
                    }

                    Transform plantingPoint = closest.GetPlantingPoint();
                    closest.Plant(heldFlower, plantingPoint.position);
                    DropFlower();
                }
            }
            else if (closest.HasPlantedFlower)
            {
                GameObject flower = closest.PickUp();
                if (flower != null)
                {
                    PickUpFlower(flower);
                }
            }
        }
    }

    private void PickUpFlower(GameObject flower)
    {
        heldFlower = flower;
        heldFlowerType = flower.GetComponent<FlowerPickup>()?.flowerType ?? "Unknown";

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = true;
            sprout.isPlanted = false;
        }

        // Align pivot to hold point
        Transform pivot = flower.transform.Find("pivot");
        Vector3 pivotOffset = pivot != null ? (flower.transform.position - pivot.position) : Vector3.zero;
        flower.transform.position = holdPoint.position + pivotOffset;

        flower.transform.SetParent(holdPoint);

        Collider2D col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        flower.SetActive(true);
    }

    public void PickUpFlowerInstance(GameObject flower)
    {
        if (!HasFlower)
        {
            PickUpFlower(flower);
        }
    }

    public GameObject GetHeldFlower() => heldFlower;

    public void DropFlower()
    {
        heldFlower = null;
        heldFlowerType = "";
    }

    private GardenSpot FindClosestGarden()
    {
        if (nearbyGardens.Count == 0) return null;

        GardenSpot closest = null;
        float minDist = float.MaxValue;

        foreach (GardenSpot garden in nearbyGardens)
        {
            float dist = Vector3.Distance(transform.position, garden.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = garden;
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
