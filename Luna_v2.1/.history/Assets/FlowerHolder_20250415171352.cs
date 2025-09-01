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

    private void Update()
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
                // Try to pick up from garden
                if (plantingPoint.childCount > 0)
                {
                    GameObject gardenFlower = plantingPoint.GetChild(0).gameObject;
                    PickUp(gardenFlower);
                }
            }
            else
            {
                // Try to plant (will swap if something is already there)
                if (plantingPoint.childCount > 0)
                {
                    GameObject flowerInSpot = plantingPoint.GetChild(0).gameObject;
                    PickUp(flowerInSpot);
                }

                Plant(heldFlower, plantingPoint);
            }

            cooldownTimer = 0f;
            canInteract = false;
        }
    }

    private void FindClosestGarden()
    {
        closestGarden = null;
        float minDistance = float.MaxValue;

        foreach (GardenSpot garden in nearbyGardens)
        {
            Transform plantingPoint = garden.transform.Find("PlantingPoint");
            if (plantingPoint == null) continue;

            float dist = Vector2.Distance(transform.position, plantingPoint.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestGarden = garden;
            }
        }
    }

    private void PickUp(GameObject flower)
    {
        heldFlower = flower;

        flower.transform.SetParent(holdPoint);
        flower.transform.position = holdPoint.position;
        flower.transform.localRotation = Quaternion.identity;

        Collider2D col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = true;

        Debug.Log($"🤲 Picked up flower: {flower.name}");
    }

    private void Plant(GameObject flower, Transform plantingPoint)
    {
        flower.transform.SetParent(plantingPoint);
        flower.transform.position = plantingPoint.position;
        flower.transform.localRotation = Quaternion.identity;

        Collider2D col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = false;

        heldFlower = null;

        Debug.Log($"🌱 Planted flower in garden: {plantingPoint.parent.name}");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            GardenSpot garden = other.GetComponent<GardenSpot>();
            if (garden != null && !nearbyGardens.Contains(garden))
            {
                nearbyGardens.Add(garden);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            GardenSpot garden = other.GetComponent<GardenSpot>();
            if (garden != null)
            {
                nearbyGardens.Remove(garden);
            }
        }
    }

    // Public API for external use (e.g. Teapot, Pickup)

    public bool HasFlower() => heldFlower != null;

    public GameObject GetHeldFlower() => heldFlower;

    public string currentFlowerType
    {
        get
        {
            if (heldFlower == null) return "";
            FlowerPickup pickup = heldFlower.GetComponent<FlowerPickup>();
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
