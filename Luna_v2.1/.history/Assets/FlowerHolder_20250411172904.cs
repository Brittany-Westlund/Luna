using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    [Header("Held Flower Settings")]
    public Transform holdPoint;
    public List<FlowerPrefabEntry> flowerPrefabs;

    private Dictionary<string, FlowerPrefabEntry> flowerPrefabMap;
    private GameObject currentFlowerObject;
    private string currentFlowerType;

    [System.Serializable]
    public class FlowerPrefabEntry
    {
        public string flowerType;
        public GameObject heldPrefab;
        public GameObject plantedPrefab; // Optional: for planting
        public Vector3 offset;
    }

    private bool nearGarden = false;
    private Transform gardenSpot;

    private void Start()
    {
        flowerPrefabMap = new Dictionary<string, FlowerPrefabEntry>();
        foreach (var entry in flowerPrefabs)
        {
            if (!flowerPrefabMap.ContainsKey(entry.flowerType))
            {
                flowerPrefabMap.Add(entry.flowerType, entry);
            }
        }
    }

    private void Update()
    {
        if (HasFlower() && nearGarden && Input.GetKeyDown(KeyCode.X))
        {
            PlantFlower(gardenSpot.position);
        }
    }

    public bool HasFlower() => currentFlowerObject != null;

    public void PickUpFlower(FlowerPickup flower)
    {
        currentFlowerType = flower.flowerType;

        if (flowerPrefabMap.TryGetValue(currentFlowerType, out FlowerPrefabEntry entry))
        {
            Vector3 spawnPosition = holdPoint.position + entry.offset;
            currentFlowerObject = Instantiate(entry.heldPrefab, spawnPosition, Quaternion.identity, holdPoint);
        }
        else
        {
            Debug.LogWarning("No held prefab found for flower type: " + currentFlowerType);
        }

        flower.DisableFlowerVisual();
        flower.DestroySelf();
    }

    public void DropFlower()
    {
        if (currentFlowerObject != null)
        {
            Destroy(currentFlowerObject);
            currentFlowerObject = null;
            currentFlowerType = "";
        }
    }

    private void PlantFlower(Vector3 position)
    {
        if (!HasFlower() || !flowerPrefabMap.ContainsKey(currentFlowerType)) return;

        GameObject plantedPrefab = flowerPrefabMap[currentFlowerType].plantedPrefab;

        if (plantedPrefab != null)
        {
            Instantiate(plantedPrefab, position, Quaternion.identity);
            Debug.Log("🌱 Planted flower: " + currentFlowerType);
        }
        else
        {
            Debug.LogWarning("No planted prefab assigned for: " + currentFlowerType);
        }

        DropFlower();
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
