using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    [Header("Held Flower")]
    public Transform holdPoint;

    private GameObject heldFlower;
    private string heldFlowerType;

    private readonly List<GardenSpot> nearbyGardens = new();
    private float interactionCooldown = 0.1f;
    private float cooldownTimer = 0f;
    private bool canInteract = true;

    void Update()
    {
        cooldownTimer += Time.deltaTime;
        if (!canInteract && cooldownTimer >= interactionCooldown)
        {
            canInteract = true;
            cooldownTimer = 0f;
        }

        if (!canInteract || Input.GetKeyDown(KeyCode.X) == false) return;

        GardenSpot closestGarden = FindClosestGarden();

        if (heldFlower && closestGarden)
        {
            Debug.Log("🌱 Attempting to plant...");
            GameObject existing = GetPlantedFlower(closestGarden);

            if (existing != null)
            {
                Debug.Log("🤲 Swapping planted flower.");
                PickUp(existing);
            }
            else
            {
                Debug.Log("🌼 Planted held flower.");
                Plant(closestGarden);
            }

            canInteract = false;
        }
        else if (!heldFlower && closestGarden)
        {
            GameObject flower = GetPlantedFlower(closestGarden);
            if (flower != null)
            {
                Debug.Log("🤲 Picking up planted flower.");
                PickUp(flower);
                canInteract = false;
            }
        }
    }

    public bool HasFlower() => heldFlower != null;

    public GameObject GetHeldFlower() => heldFlower;

    public string currentFlowerType => heldFlowerType;

    public void PickUpFlowerInstance(GameObject flower)
    {
        PickUp(flower);
    }

    public void DropFlower()
    {
        heldFlower = null;
        heldFlowerType = "";
    }

    private void PickUp(GameObject flower)
    {
        heldFlower = flower;
        heldFlowerType = flower.GetComponent<FlowerPickup>()?.flowerType ?? "Unknown";

        flower.transform.SetParent(null);
        flower.SetActive(true);

        Transform pivot = flower.transform.Find("pivot");
        if (pivot != null)
        {
            Vector3 offset = flower.transform.position - pivot.position;
            flower.transform.position = holdPoint.position + offset;
        }
        else
        {
            flower.transform.position = holdPoint.position;
        }

        flower.transform.SetParent(holdPoint);
        flower.GetComponent<Collider2D>().enabled = false;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = true;
    }

    private void Plant(GardenSpot garden)
    {
        if (!heldFlower) return;

        Transform plantingPoint = garden.transform.Find("PlantingPoint");
        Transform pivot = heldFlower.transform.Find("pivot");

        if (plantingPoint == null || pivot == null)
        {
            Debug.LogWarning("⚠️ Missing PlantingPoint or pivot.");
            return;
        }

        Vector3 offset = heldFlower.transform.position - pivot.position;

        heldFlower.transform.SetParent(null);
        heldFlower.transform.position = plantingPoint.position + offset;
        heldFlower.GetComponent<Collider2D>().enabled = true;

        var sprout = heldFlower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = false;

        DropFlower();
    }

    private GameObject GetPlantedFlower(GardenSpot garden)
    {
        foreach (Transform child in garden.transform)
        {
            if (child.CompareTag("Flower")) return child.gameObject;
        }
        return null;
    }

    private GardenSpot FindClosestGarden()
    {
        GardenSpot closest = null;
        float minDist = float.MaxValue;

        foreach (GardenSpot spot in nearbyGardens)
        {
            float dist = Vector2.Distance(transform.position, spot.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = spot;
            }
        }

        return closest;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GardenSpot spot = other.GetComponent<GardenSpot>();
        if (spot != null && !nearbyGardens.Contains(spot))
        {
            nearbyGardens.Add(spot);
            spot.SetHighlight(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        GardenSpot spot = other.GetComponent<GardenSpot>();
        if (spot != null && nearbyGardens.Contains(spot))
        {
            nearbyGardens.Remove(spot);
            spot.SetHighlight(false);
        }
    }
}
