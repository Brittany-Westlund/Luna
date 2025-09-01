using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    [Header("Held Flower Settings")]
    public Transform holdPoint;
    public List<FlowerPrefabEntry> flowerPrefabs;

    private GameObject currentFlowerObject;
    private string heldFlowerType;

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
            GameObject plantedFlower = GetPlantedFlower(plantingPoint);

            Debug.Log($"[🌿 INTERACT] HasFlower: {HasFlower()}, Planted: {plantedFlower?.name}, Held: {currentFlowerObject?.name}");

            if (HasFlower())
            {
                if (plantedFlower != null)
                {
                    Debug.Log("[🧹 CLEAR] Removing planted flower...");
                    plantedFlower.transform.SetParent(null);
                    plantedFlower.SetActive(true);
                }

                Debug.Log("[🌱 PLANTING] Planting held flower.");
                StartCoroutine(SafePlant(closestGarden));
            }
            else
            {
                if (plantedFlower != null)
                {
                    Debug.Log("[🤲 PICKUP] Picking up planted flower.");
                    PickUpFlowerInstance(plantedFlower);
                    Debug.Log($"[🤲 AFTER PICKUP] currentFlowerObject: {currentFlowerObject?.name}");
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

    private void UpdateClosestGarden()
    {
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

    private GameObject GetPlantedFlower(Transform plantingPoint)
    {
        if (plantingPoint.childCount == 0) return null;
        return plantingPoint.GetChild(0).gameObject;
    }

    public bool HasFlower() => currentFlowerObject != null;

    public GameObject GetHeldFlower() => currentFlowerObject;

    public string currentFlowerType => heldFlowerType;

    public void DropFlower()
    {
        Debug.Log($"[🧼 DROP] Dropping flower: {currentFlowerObject?.name}");
        currentFlowerObject = null;
        heldFlowerType = "";
    }

    public void PickUpFlowerInstance(GameObject flower)
    {
        currentFlowerObject = flower;
        heldFlowerType = flower.GetComponent<FlowerPickup>()?.flowerType ?? "Unknown";

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = true;

        Transform pivot = flower.transform.Find("pivot");
        Vector3 offset = pivot != null ? flower.transform.position - pivot.position : Vector3.zero;

        flower.transform.position = holdPoint.position + offset;
        flower.transform.SetParent(holdPoint);
        flower.transform.localScale = Vector3.one; // ✅ fix skew
        flower.transform.localRotation = Quaternion.identity; // ✅ fix tilt
        flower.GetComponent<Collider2D>().enabled = false;

        Debug.Log($"[🧠 DEBUG] PickUpFlowerInstance: {flower.name} now held.");
    }

    private IEnumerator SafePlant(GardenSpot garden)
    {
        GameObject flowerToPlant = currentFlowerObject;

        if (flowerToPlant == null)
        {
            Debug.LogWarning("❌ Tried to plant null flower.");
            yield break;
        }

        Transform plantingPoint = garden.transform.Find("PlantingPoint");
        if (plantingPoint == null)
        {
            Debug.LogWarning("❌ No planting point on garden.");
            yield break;
        }

        Transform pivot = flowerToPlant.transform.Find("pivot");
        Vector3 offset = pivot != null ? flowerToPlant.transform.position - pivot.position : Vector3.zero;

        flowerToPlant.transform.position = plantingPoint.position + offset;
        flowerToPlant.transform.SetParent(plantingPoint);
        flowerToPlant.transform.localScale = Vector3.one; // ✅ fix skew
        flowerToPlant.transform.localRotation = Quaternion.identity; // ✅ fix tilt
        flowerToPlant.GetComponent<Collider2D>().enabled = true;

        var sprout = flowerToPlant.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = false;

        yield return new WaitForEndOfFrame();
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
                {
                    closestGarden = null;
                }
            }
        }
    }
}
