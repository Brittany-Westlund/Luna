using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    [Header("Held Flower Settings")]
    public Transform holdPoint;
    public List<FlowerPrefabEntry> flowerPrefabs;

    private GameObject currentFlowerObject;
    public string currentFlowerType;

    private bool nearGarden = false;
    private Transform gardenSpot;

    [System.Serializable]
    public class FlowerPrefabEntry
    {
        public string flowerType;
        public Vector3 offset;
    }

    private void Start()
    {
        // Optional: Initialize flowerPrefabMap if still using it elsewhere
    }

    public GameObject GetHeldFlower()
{
    return currentFlowerObject;
}


    private void Update()
    {
        if (HasFlower() && nearGarden && Input.GetKeyDown(KeyCode.X))
        {
            PlantFlower();
        }
    }

    public bool HasFlower() => currentFlowerObject != null;

    public void PickUpFlowerInstance(GameObject flower)
    {
        currentFlowerObject = flower;
        currentFlowerType = flower.GetComponent<FlowerPickup>()?.flowerType ?? "Unknown";

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = true;
        }

        flower.transform.SetParent(holdPoint);
        flower.transform.localPosition = Vector3.zero;
        flower.GetComponent<Collider2D>().enabled = false;
    }

    public void DropFlower()
    {
        if (currentFlowerObject != null)
        {
            currentFlowerObject = null;
            currentFlowerType = "";
        }
    }

    private void PlantFlower()
    {
        if (!HasFlower()) return;

        // 🔍 Locate the planting point on the garden spot
        Transform plantingPoint = gardenSpot.Find("PlantingPoint");
        if (plantingPoint == null)
        {
            Debug.LogWarning("No PlantingPoint found on garden spot!");
            return;
        }

        // 🔍 Locate the pivot point on the flower
        Transform flowerPivot = currentFlowerObject.transform.Find("pivot");
        if (flowerPivot == null)
        {
            Debug.LogWarning("No 'pivot' child found on flower: " + currentFlowerObject.name);
            return;
        }

        // 📐 Align flower so that its pivot matches the planting point
        Vector3 pivotOffset = currentFlowerObject.transform.position - flowerPivot.position;
        currentFlowerObject.transform.SetParent(null);
        currentFlowerObject.transform.position = plantingPoint.position + pivotOffset;

        currentFlowerObject.GetComponent<Collider2D>().enabled = true;

        var sprout = currentFlowerObject.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = false;
        }

        DropFlower();
        Debug.Log("🌱 Planted existing flower at planting point: " + currentFlowerType);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            nearGarden = true;
            gardenSpot = other.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            nearGarden = false;
            gardenSpot = null;
        }
    }
}
