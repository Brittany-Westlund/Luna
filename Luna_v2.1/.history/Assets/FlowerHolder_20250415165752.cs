using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    [Header("Held Flower Settings")]
    public Transform holdPoint;

    private GameObject heldFlower;
    private List<GardenSpot> nearbyGardens = new List<GardenSpot>();
    private GardenSpot closestGarden;

    private bool canInteract = true;
    private float interactionCooldown = 0.1f;
    private float cooldownTimer = 0f;

    private void Update()
    {
        FindClosestGarden();

        if (canInteract && closestGarden != null && Input.GetKeyDown(KeyCode.X))
        {
            Transform plantingPoint = closestGarden.transform.Find("PlantingPoint");
            GameObject flowerInSpot = GetPlantedFlower(plantingPoint);

            if (HasFlower())
            {
                if (flowerInSpot != null)
                {
                    Remove(flowerInSpot);
                }

                Plant(heldFlower, plantingPoint);
            }
            else
            {
                if (flowerInSpot != null)
                {
                    PickUp(flowerInSpot);
                }
            }

            canInteract = false;
        }

        if (!canInteract)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= interactionCooldown)
            {
                canInteract = true;
                cooldownTimer = 0f;
            }
        }
    }

    private void PickUp(GameObject flower)
    {
        heldFlower = flower;

        Transform pivot = flower.transform.Find("pivot");
        Vector3 offset = pivot ? flower.transform.position - pivot.position : Vector3.zero;

        flower.transform.SetParent(holdPoint);
        flower.transform.position = holdPoint.position + offset;
        flower.transform.localRotation = Quaternion.identity;
        flower.transform.localScale = Vector3.one;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = true;

        flower.GetComponent<Collider2D>().enabled = false;
    }

    private void Plant(GameObject flower, Transform plantingPoint)
    {
        heldFlower = null;

        Transform pivot = flower.transform.Find("pivot");
        Vector3 offset = pivot ? flower.transform.position - pivot.position : Vector3.zero;

        flower.transform.SetParent(plantingPoint);
        flower.transform.position = plantingPoint.position + offset;
        flower.transform.localRotation = Quaternion.identity;
        flower.transform.localScale = Vector3.one;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = false;

        flower.GetComponent<Collider2D>().enabled = true;
    }

    private void Remove(GameObject flower)
    {
        flower.transform.SetParent(null);
        flower.SetActive(true);
    }

    private GameObject GetPlantedFlower(Transform plantingPoint)
    {
        if (plantingPoint.childCount == 0) return null;
        return plantingPoint.GetChild(0).gameObject;
    }

    private void FindClosestGarden()
    {
        closestGarden = null;
        float minDist = float.MaxValue;

        foreach (GardenSpot garden in nearbyGardens)
        {
            Transform point = garden.transform.Find("PlantingPoint");
            if (point == null) continue;

            float dist = Vector2.Distance(transform.position, point.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestGarden = garden;
            }
        }
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
            if (garden != null)
                nearbyGardens.Remove(garden);
        }
    }

    // 🧃 Public methods for use by Teapot and Pickup

    public bool HasFlower()
    {
        return heldFlower != null;
    }

    public GameObject GetHeldFlower()
    {
        return heldFlower;
    }

    public string currentFlowerType
    {
        get
        {
            if (heldFlower == null) return "";
            var pickup = heldFlower.GetComponent<FlowerPickup>();
            return pickup != null ? pickup.flowerType : "";
        }
    }

    public void DropFlower()
    {
        if (heldFlower != null)
        {
            heldFlower.transform.SetParent(null);
            heldFlower.GetComponent<Collider2D>().enabled = true;
            heldFlower = null;
        }
    }

    public void PickUpFlowerInstance(GameObject flower)
    {
        PickUp(flower);
    }
}
