using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    [Header("Held Flower Settings")]
    public Transform holdPoint;
    public List<FlowerPrefabEntry> flowerPrefabs;

    private GameObject currentFlowerObject;
    public string currentFlowerType;

    private List<GardenSpot> nearbyGardens = new List<GardenSpot>();
    private GardenSpot closestGarden;

    private bool canInteract = true;
    private float interactionCooldown = 0.1f;
    private float cooldownTimer = 0f;

    [System.Serializable]
    public class FlowerPrefabEntry
    {
        public string flowerType;
        public Vector3 offset;
    }

    private void Start()
    {
        if (holdPoint == null)
        {
            holdPoint = transform.Find("HoldPoint");
            if (holdPoint == null)
            {
                Debug.LogWarning("⚠️ No HoldPoint assigned or found on FlowerHolder.");
            }
        }
    }

    private void Update()
    {
        UpdateClosestGardenHighlight();

        if (canInteract && HasFlower() && closestGarden != null && Input.GetKeyDown(KeyCode.X))
        {
            PlantFlower(closestGarden);
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

    private void UpdateClosestGardenHighlight()
    {
        if (!HasFlower())
        {
            // If not holding a flower, turn all highlights off
            foreach (var g in nearbyGardens)
            {
                g.SetHighlight(false);
            }
            closestGarden = null;
            return;
        }

        float minDist = float.MaxValue;
        GardenSpot best = null;

        foreach (var g in nearbyGardens)
        {
            if (g == null || g.isPlanted) continue;

            float dist = Vector2.Distance(transform.position, g.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                best = g;
            }
        }

        if (closestGarden != null && closestGarden != best)
        {
            closestGarden.SetHighlight(false);
        }

        closestGarden = best;

        if (closestGarden != null)
        {
            closestGarden.SetHighlight(true);
        }
    }

    public GameObject GetHeldFlower() => currentFlowerObject;

    public bool HasFlower() => currentFlowerObject != null;

    public void PickUpFlowerInstance(GameObject flower)
    {
        currentFlowerObject = flower;
        currentFlowerType = flower.GetComponent<FlowerPickup>()?.flowerType ?? "Unknown";

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = true;

        Transform flowerPivot = flower.transform.Find("pivot");
        if (flowerPivot == null)
        {
            flower.transform.SetParent(holdPoint);
            flower.transform.localPosition = Vector3.zero;
            flower.GetComponent<Collider2D>().enabled = false;
            return;
        }

        Vector3 offset = flower.transform.position - flowerPivot.position;
        flower.transform.position = holdPoint.position + offset;

        flower.transform.SetParent(holdPoint);
        flower.GetComponent<Collider2D>().enabled = false;
    }

    public void DropFlower()
    {
        currentFlowerObject = null;
        currentFlowerType = "";
    }

    private void PlantFlower(GardenSpot garden)
    {
        if (garden.isPlanted || !HasFlower()) return;

        Transform plantingPoint = garden.transform.Find("PlantingPoint");
        if (plantingPoint == null)
        {
            Debug.LogWarning("❌ No PlantingPoint found in garden.");
            return;
        }

        Transform flowerPivot = currentFlowerObject.transform.Find("pivot");
        if (flowerPivot == null)
        {
            Debug.LogWarning("❌ No pivot found on flower.");
            return;
        }

        Vector3 pivotOffset = currentFlowerObject.transform.position - flowerPivot.position;
        currentFlowerObject.transform.SetParent(null);
        currentFlowerObject.transform.position = plantingPoint.position + pivotOffset;
        currentFlowerObject.GetComponent<Collider2D>().enabled = true;

        var sprout = currentFlowerObject.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = false;

        garden.isPlanted = true;
        garden.SetHighlight(false);

        DropFlower();
        Debug.Log("🌱 Planted flower in: " + garden.name);
    }

    private void OnTriggerStay2D(Collider2D other)
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
            if (garden != null && nearbyGardens.Contains(garden))
            {
                garden.SetHighlight(false);
                nearbyGardens.Remove(garden);
            }
        }
    }
}
