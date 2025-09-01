using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    public Transform holdPoint;

    private GameObject heldFlower;
    private string heldFlowerType = "";
    private List<GardenSpot> nearbyGardens = new List<GardenSpot>();

    public string CurrentFlowerType => heldFlowerType;
    public bool HasFlower => heldFlower != null;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            GardenSpot closest = FindClosestGarden();
            if (closest == null) return;

            if (HasFlower)
            {
                if (closest.HasPlantedFlower)
                {
                    GameObject replaced = closest.PickUp();
                    if (replaced != null) replaced.transform.SetParent(null); // Just detach
                }

                Transform plantingPoint = closest.GetPlantingPoint();
                closest.Plant(heldFlower, plantingPoint.position);
                DropFlower();
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

    public GameObject GetHeldFlower() => heldFlower;

    public void PickUpFlowerInstance(GameObject flower)
    {
        PickUpFlower(flower);
    }

    public void DropFlower()
    {
        heldFlower = null;
        heldFlowerType = "";
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

        Transform pivot = flower.transform.Find("pivot");
        if (pivot != null)
        {
            Vector3 pivotOffset = flower.transform.position - pivot.position;
            flower.transform.position = holdPoint.position + pivotOffset;
        }
        else
        {
            flower.transform.position = holdPoint.position;
        }

        flower.transform.SetParent(holdPoint);

        var col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        flower.SetActive(true);
    }

    private GardenSpot FindClosestGarden()
    {
        if (nearbyGardens.Count == 0) return null;

        GardenSpot closest = null;
        float minDistance = float.MaxValue;

        foreach (var garden in nearbyGardens)
        {
            float dist = Vector3.Distance(transform.position, garden.transform.position);
            if (dist < minDistance)
            {
                minDistance = dist;
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
