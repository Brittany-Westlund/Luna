using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    [Header("Held Flower Settings")]
    public Transform holdPoint;

    private GameObject currentFlowerObject;
    private string heldFlowerType = "";

    private GardenSpot closestGarden = null;
    private Dictionary<GardenSpot, GameObject> plantedFlowers = new Dictionary<GardenSpot, GameObject>();

    private float interactionCooldown = 0.1f;
    private float cooldownTimer = 0f;
    private bool canInteract = true;

    void Update()
    {
        cooldownTimer += Time.deltaTime;
        if (cooldownTimer >= interactionCooldown)
        {
            canInteract = true;
        }

        if (Input.GetKeyDown(KeyCode.X) && canInteract)
        {
            if (HasFlower() && closestGarden != null)
            {
                // 🌱 Replant logic
                PlantFlower(closestGarden);
            }
            else if (!HasFlower() && closestGarden != null)
            {
                // 🤲 Pick up planted flower
                GameObject planted = GetPlantedFlower(closestGarden);
                if (planted != null)
                {
                    PickUpFlowerInstance(planted);
                    plantedFlowers.Remove(closestGarden);
                }
            }

            canInteract = false;
            cooldownTimer = 0f;
        }
    }

    public bool HasFlower()
    {
        return currentFlowerObject != null;
    }

    public GameObject GetHeldFlower()
    {
        return currentFlowerObject;
    }

    public string currentFlowerType
    {
        get { return heldFlowerType; }
    }

    public void PickUpFlowerInstance(GameObject flower)
    {
        currentFlowerObject = flower;
        heldFlowerType = flower.GetComponent<FlowerPickup>()?.flowerType ?? "Unknown";

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = true;
        }

        // 🌀 Align pivot to hold point
        Transform pivot = flower.transform.Find("pivot");
        if (pivot != null)
        {
            Vector3 offset = flower.transform.position - pivot.position;
            flower.transform.position = holdPoint.position + offset;
        }
        else
        {
            Debug.LogWarning("⚠️ Flower missing pivot: " + flower.name);
            flower.transform.position = holdPoint.position;
        }

        flower.transform.SetParent(holdPoint);
        flower.GetComponent<Collider2D>().enabled = false;
    }

    public void DropFlower()
    {
        if (currentFlowerObject != null)
        {
            currentFlowerObject = null;
            heldFlowerType = "";
        }
    }

    private void PlantFlower(GardenSpot spot)
    {
        if (!HasFlower()) return;

        Transform plantingPoint = spot.transform.Find("PlantingPoint");
        if (plantingPoint == null)
        {
            Debug.LogWarning("⚠️ No PlantingPoint found on: " + spot.name);
            return;
        }

        // 🌼 If something's already planted here, remove it
        if (plantedFlowers.TryGetValue(spot, out GameObject existing))
        {
            Destroy(existing);
        }

        Transform pivot = currentFlowerObject.transform.Find("pivot");
        Vector3 offset = pivot != null ? currentFlowerObject.transform.position - pivot.position : Vector3.zero;

        currentFlowerObject.transform.SetParent(null);
        currentFlowerObject.transform.position = plantingPoint.position + offset;
        currentFlowerObject.GetComponent<Collider2D>().enabled = true;

        var sprout = currentFlowerObject.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = false;
        }

        plantedFlowers[spot] = currentFlowerObject;
        DropFlower();

        Debug.Log("🌱 Planted held flower in garden.");
    }

    private GameObject GetPlantedFlower(GardenSpot spot)
    {
        if (plantedFlowers.TryGetValue(spot, out GameObject flower))
        {
            return flower;
        }
        return null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GardenSpot spot = other.GetComponent<GardenSpot>();
        if (spot != null)
        {
            closestGarden = spot;
            closestGarden.SetHighlight(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        GardenSpot spot = other.GetComponent<GardenSpot>();
        if (spot != null && spot == closestGarden)
        {
            closestGarden.SetHighlight(false);
            closestGarden = null;
        }
    }
}
