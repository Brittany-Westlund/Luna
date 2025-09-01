using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    [Header("Holding")]
    public Transform holdPoint;

    private GameObject heldFlower;
    private List<GardenSpot> nearbyGardens = new List<GardenSpot>();
    private GardenSpot closestGarden;

    private float interactCooldown = 0.1f;
    private float cooldownTimer = 0f;
    private bool canInteract = true;

    private void Update()
    {
        FindClosestGarden();

        // 🌸 Handle pickup/plant interaction
        if (canInteract && closestGarden != null && Input.GetKeyDown(KeyCode.X))
        {
            Transform plantingPoint = closestGarden.transform.Find("PlantingPoint");

            if (heldFlower == null)
            {
                // Try to pick up the flower from the garden
                if (plantingPoint.childCount > 0)
                {
                    GameObject flower = plantingPoint.GetChild(0).gameObject;
                    PickUp(flower);
                }
            }
            else
            {
                // If there's a flower there, swap it
                if (plantingPoint.childCount > 0)
                {
                    GameObject existing = plantingPoint.GetChild(0).gameObject;
                    PickUp(existing); // First grab the existing one
                }

                Plant(heldFlower, plantingPoint); // Then plant the one she's holding
            }

            canInteract = false;
        }

        // 🔄 Handle cooldown
        if (!canInteract)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= interactCooldown)
            {
                canInteract = true;
                cooldownTimer = 0f;
            }
        }
    }

    private void PickUp(GameObject flower)
    {
        heldFlower = flower;
        flower.transform.SetParent(holdPoint);
        flower.transform.position = holdPoint.position;
        flower.transform.localRotation = Quaternion.identity;
        flower.GetComponent<Collider2D>().enabled = false;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = true;

        Debug.Log($"🤲 Picked up {flower.name}");
    }

    private void Plant(GameObject flower, Transform plantingPoint)
    {
        flower.transform.SetParent(plantingPoint);
        flower.transform.position = plantingPoint.position;
        flower.transform.localRotation = Quaternion.identity;
        flower.GetComponent<Collider2D>().enabled = true;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = false;

        heldFlower = null;

        Debug.Log($"🌱 Planted flower in {plantingPoint.parent.name}");
    }

    private void FindClosestGarden()
    {
        closestGarden = null;
        float closestDistance = float.MaxValue;

        foreach (GardenSpot garden in nearbyGardens)
        {
            Transform point = garden.transform.Find("PlantingPoint");
            if (point == null) continue;

            float dist = Vector2.Distance(transform.position, point.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
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

    // 🧃 Public accessors

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
