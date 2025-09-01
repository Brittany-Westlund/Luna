using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
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

    private void PlantFlower(Vector3 position)
    {
        if (!HasFlower()) return;

        currentFlowerObject.transform.SetParent(null);
        currentFlowerObject.transform.position = position;
        currentFlowerObject.GetComponent<Collider2D>().enabled = true;

        var sprout = currentFlowerObject.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = false;
        }

        DropFlower();
        Debug.Log("🌱 Planted existing flower: " + currentFlowerType);
    }

    private void Update()
    {
        if (HasFlower() && nearGarden && Input.GetKeyDown(KeyCode.X))
        {
            PlantFlower(gardenSpot.position);
        }
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
