using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    [Header("Holding Settings")]
    public Transform holdPoint;

    private GameObject heldFlower;
    private List<GardenSpot> nearbyGardens = new List<GardenSpot>();
    private GardenSpot closestGarden;

    private float interactCooldown = 0.15f;
    private float cooldownTimer = 0f;
    private bool canInteract = true;

    void Update()
    {
        cooldownTimer += Time.deltaTime;
        if (cooldownTimer >= interactCooldown)
        {
            canInteract = true;
        }

        FindClosestGarden();

        if (canInteract && closestGarden != null && Input.GetKeyDown(KeyCode.X))
        {
            Transform plantingPoint = closestGarden.transform.Find("PlantingPoint");

            if (heldFlower == null)
            {
                // Try to pick up existing flower
                GameObject existing = GetPlantedFlower(plantingPoint);
                if (existing != null)
                {
                    PickUp(existing);
                }
            }
            else
            {
                // If flower exists in garden, swap
                GameObject existing = GetPlantedFlower(plantingPoint);
                if (existing != null)
                {
                    PickUp(existing);
                }

                PlantHeldFlower(plantingPoint);
            }

            cooldownTimer = 0f;
            canInteract = false;
        }
    }

    private GameObject GetPlantedFlower(Transform plantingPoint)
    {
        if (plantingPoint.childCount > 0)
        {
            return plantingPoint.GetChild(0).gameObject;
        }
        return null;
    }

    private void PlantHeldFlower(Transform plantingPoint)
    {
        if (heldFlower == null) return;

        // Align pivot to planting point
        Transform pivot = heldFlower.transform.Find("pivot");
        Vector3 offset = Vector3.zero;

        if (pivot != null)
        {
            offset = heldFlower.transform.position - pivot.position;
        }

        heldFlower.transform.SetParent(plantingPoint);
        heldFlower.transform.position = plantingPoint.position + offset;
        heldFlower.transform.localRotation = Quaternion.identity;

        var col = heldFlower.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        var sprout = heldFlower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = false;

        Debug.Log($"🌱 Planted flower {heldFlower.name} in {plantingPoint.parent.name}");

        heldFlower = null;
    }

    private void PickUp(GameObject flower)
    {
        heldFlower = flower;

        // Align pivot to holdPoint
        Transform pivot = flower.transform.Find("pivot");
        Vector3 offset = Vector3.zero;

        if (pivot != null)
        {
            offset = flower.transform.position - pivot.position;
        }

        flower.transform.SetParent(holdPoint);
        flower.transform.position = holdPoint.position + offset;
        flower.transform.localRotation = Quaternion.identity;

        var col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = true;

        Debug.Log($"🤲 Picked up flower: {flower.name}");
    }

    private void FindClosestGarden()
    {
        closestGarden = null;
        float closestDist = float.MaxValue;

        foreach (GardenSpot g in nearbyGardens)
        {
            Transform point = g.transform.Find("PlantingPoint");
            if (point == null) continue;

            float dist = Vector2.Distance(transform.position, point.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestGarden = g;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            GardenSpot spot = other.GetComponent<GardenSpot>();
            if (spot != null && !nearbyGardens.Contains(spot))
            {
                nearbyGardens.Add(spot);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            GardenSpot spot = other.GetComponent<GardenSpot>();
            if (spot != null)
            {
                nearbyGardens.Remove(spot);
            }
        }
    }

    // 🌼 Public Accessors

    public bool HasFlower() => heldFlower != null;

    public GameObject GetHeldFlower() => heldFlower;

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
