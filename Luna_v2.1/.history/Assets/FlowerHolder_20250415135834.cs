using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    [Header("Held Flower Settings")]
    public Transform holdPoint;
    public List<FlowerPrefabEntry> flowerPrefabs;

    private GameObject currentFlowerObject;
    public string currentFlowerType;

    private Transform gardenSpot;

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
        if (canInteract && gardenSpot != null && Input.GetKeyDown(KeyCode.X) && HasFlower())
        {
            PlantFlower();
            canInteract = false;
        }

        // Cooldown handling
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

    public GameObject GetHeldFlower()
    {
        return currentFlowerObject;
    }

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
            Debug.LogWarning("No 'pivot' found on flower: " + flower.name);
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

    private void PlantFlower()
    {
        if (!HasFlower() || gardenSpot == null) return;

        Transform plantingPoint = gardenSpot.Find("PlantingPoint");
        if (plantingPoint == null)
        {
            Debug.LogWarning("❌ No PlantingPoint found on garden spot.");
            return;
        }

        Transform flowerPivot = currentFlowerObject.transform.Find("pivot");
        if (flowerPivot == null)
        {
            Debug.LogWarning("❌ No pivot found on flower: " + currentFlowerObject.name);
            return;
        }

        Vector3 pivotOffset = currentFlowerObject.transform.position - flowerPivot.position;
        currentFlowerObject.transform.SetParent(null);
        currentFlowerObject.transform.position = plantingPoint.position + pivotOffset;
        currentFlowerObject.GetComponent<Collider2D>().enabled = true;

        var sprout = currentFlowerObject.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = false;

        DropFlower();
        Debug.Log("🌱 Planted flower: " + currentFlowerType);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            gardenSpot = other.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Garden") && other.transform == gardenSpot)
        {
            gardenSpot = null;
        }
    }
}
