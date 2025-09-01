using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    [Header("Held Flower Settings")]
    public Transform holdPoint;

    private GameObject currentFlowerObject;
    private List<GardenSpot> nearbyGardens = new List<GardenSpot>();
    private GardenSpot closestGarden;
    public bool HasFlower() => currentFlowerObject != null;

public GameObject GetHeldFlower() => currentFlowerObject;

public string currentFlowerType => currentFlowerType_Internal;
private string currentFlowerType_Internal;

    public void DropFlower()
    {
        currentFlowerObject = null;
        currentFlowerType_Internal = "";
    }

    public void PickUpFlowerInstance(GameObject flower)
    {
        currentFlowerObject = flower;
        currentFlowerType_Internal = flower.GetComponent<FlowerPickup>()?.flowerType ?? "Unknown";

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = true;

        Transform pivot = flower.transform.Find("pivot");
        Vector3 offset = (pivot != null) ? flower.transform.position - pivot.position : Vector3.zero;

        flower.transform.position = holdPoint.position + offset;
        flower.transform.SetParent(holdPoint);
        flower.GetComponent<Collider2D>().enabled = false;
    }


    private void Update()
    {
        UpdateClosestGarden();

        if (Input.GetKeyDown(KeyCode.X) && closestGarden != null)
        {
            Transform plantingPoint = closestGarden.transform.Find("PlantingPoint");

            if (currentFlowerObject == null && plantingPoint.childCount > 0)
            {
                // Pick up existing flower
                GameObject existingFlower = plantingPoint.GetChild(0).gameObject;
                PickUpFlower(existingFlower);
            }
            else if (currentFlowerObject != null)
            {
                // If a flower is already planted, pick it up first
                if (plantingPoint.childCount > 0)
                {
                    GameObject existingFlower = plantingPoint.GetChild(0).gameObject;
                    PickUpFlower(existingFlower);
                }

                // Plant current flower
                PlantFlower(plantingPoint);
            }
        }
    }

    private void UpdateClosestGarden()
    {
        GardenSpot previousClosest = closestGarden;
        closestGarden = null;
        float minDistance = float.MaxValue;

        foreach (GardenSpot garden in nearbyGardens)
        {
            float distance = Vector2.Distance(transform.position, garden.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestGarden = garden;
            }
        }

        if (previousClosest != null && previousClosest != closestGarden)
            previousClosest.SetHighlight(false);

        if (closestGarden != null)
            closestGarden.SetHighlight(currentFlowerObject != null);
    }

    private void PickUpFlower(GameObject flower)
    {
        currentFlowerObject = flower;
        flower.transform.SetParent(holdPoint);
        flower.transform.localPosition = Vector3.zero;
        flower.GetComponent<Collider2D>().enabled = false;
    }

    private void PlantFlower(Transform plantingPoint)
    {
        currentFlowerObject.transform.SetParent(plantingPoint);
        currentFlowerObject.transform.localPosition = Vector3.zero;
        currentFlowerObject.GetComponent<Collider2D>().enabled = true;
        currentFlowerObject = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            GardenSpot garden = other.GetComponent<GardenSpot>();
            if (garden != null && !nearbyGardens.Contains(garden))
                nearbyGardens.Add(garden);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            GardenSpot garden = other.GetComponent<GardenSpot>();
            if (garden != null && nearbyGardens.Contains(garden))
            {
                garden.SetHighlight(false);
                nearbyGardens.Remove(garden);
            }
        }
    }
}
