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
                Debug.LogWarning("⚠️ No HoldPoint found on FlowerHolder.");
        }
    }

    private void Update()
    {
        UpdateClosestGarden();

        if (canInteract && closestGarden != null && Input.GetKeyDown(KeyCode.X))
        {
            Transform plantingPoint = closestGarden.transform.Find("PlantingPoint");

            if (!HasFlower() && plantingPoint.childCount > 0)
            {
                GameObject existing = plantingPoint.GetChild(0).gameObject;
                PickUpFlowerInstance(existing);
                Debug.Log($"🌼 Picked up flower from {closestGarden.name}");
            }
            else if (HasFlower())
            {
                if (plantingPoint.childCount > 0)
                {
                    GameObject existing = plantingPoint.GetChild(0).gameObject;
                    existing.transform.SetParent(null); // free it first
                }

                PlantFlower(closestGarden);
                Debug.Log($"🌱 Planted flower in {closestGarden.name}");
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

    private void UpdateClosestGarden()
    {
        GardenSpot previous = closestGarden;
        closestGarden = null;

        float minDist = float.MaxValue;
        foreach (GardenSpot garden in nearbyGardens)
        {
            if (garden == null) continue;

            Transform point = garden.transform.Find("PlantingPoint");
            if (point == null) continue;

            float dist = Vector2.Distance(transform.position, point.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestGarden = garden;
            }
        }

        foreach (GardenSpot g in nearbyGardens)
        {
            g.SetHighlight(g == closestGarden && HasFlower());
        }
    }

    public bool HasFlower() => currentFlowerObject != null;

    public GameObject GetHeldFlower() => currentFlowerObject;

    public void PickUpFlowerInstance(GameObject flower)
    {
        currentFlowerObject = flower;
        currentFlowerType = flower.GetComponent<FlowerPickup>()?.flowerType ?? "Unknown";

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = true;

        Transform pivot = flower.transform.Find("pivot");
        Vector3 offset = (pivot != null) ? flower.transform.position - pivot.position : Vector3.zero;

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
        Transform plantingPoint = garden.transform.Find("PlantingPoint");
        if (plantingPoint == null || !HasFlower()) return;

        Transform pivot = currentFlowerObject.transform.Find("pivot");
        Vector3 offset = (pivot != null) ? currentFlowerObject.transform.position - pivot.position : Vector3.zero;

        currentFlowerObject.transform.SetParent(plantingPoint);
        currentFlowerObject.transform.position = plantingPoint.position + offset;
        currentFlowerObject.GetComponent<Collider2D>().enabled = true;

        var sprout = currentFlowerObject.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = false;

        DropFlower();
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
                garden.SetHighlight(false);
                nearbyGardens.Remove(garden);

                if (garden == closestGarden)
                    closestGarden = null;
            }
        }
    }
}
