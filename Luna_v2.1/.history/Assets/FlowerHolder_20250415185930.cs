using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    [Header("Held Flower Settings")]
    public Transform holdPoint;

    private GameObject currentFlowerObject;
    private string heldFlowerType = "";

    private GardenSpot closestGarden = null;
    private Dictionary<GameObject, GameObject> plantedFlowers = new Dictionary<GameObject, GameObject>();

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
                PlantFlower(closestGarden);
            }
            else if (!HasFlower() && closestGarden != null)
            {
                GameObject planted = GetPlantedFlower(closestGarden);
                if (planted != null)
                {
                    PickUpFlowerInstance(planted);
                    plantedFlowers.Remove(closestGarden.gameObject);
                }
            }

            canInteract = false;
            cooldownTimer = 0f;
        }
    }

    public bool HasFlower() => currentFlowerObject != null;

    public GameObject GetHeldFlower() => currentFlowerObject;

    public string currentFlowerType => heldFlowerType;

    public void PickUpFlowerInstance(GameObject flower)
    {
        currentFlowerObject = flower;
        heldFlowerType = flower.GetComponent<FlowerPickup>()?.flowerType ?? "Unknown";

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = true;
        }

        flower.transform.SetParent(holdPoint, worldPositionStays: false);
        flower.transform.localPosition = Vector3.zero;
        flower.transform.localRotation = Quaternion.identity;

        // Restore original scale
        flower.transform.localScale = flower.GetComponent<FlowerPickup>().originalScale;

        flower.GetComponent<Collider2D>().enabled = false;
    }

    public void DropFlower()
    {
        currentFlowerObject = null;
        heldFlowerType = "";
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

        if (plantedFlowers.TryGetValue(spot.gameObject, out GameObject existing))
        {
            plantedFlowers.Remove(spot.gameObject);
        }

        currentFlowerObject.transform.SetParent(null);
        currentFlowerObject.transform.position = plantingPoint.position;
        currentFlowerObject.transform.rotation = Quaternion.identity;

        // Restore original scale again
        currentFlowerObject.transform.localScale = currentFlowerObject.GetComponent<FlowerPickup>().originalScale;
        currentFlowerObject.GetComponent<Collider2D>().enabled = true;

        var sprout = currentFlowerObject.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = false;
        }

        plantedFlowers[spot.gameObject] = currentFlowerObject;

        DropFlower();
    }

    private GameObject GetPlantedFlower(GardenSpot spot)
    {
        if (plantedFlowers.TryGetValue(spot.gameObject, out GameObject flower))
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
