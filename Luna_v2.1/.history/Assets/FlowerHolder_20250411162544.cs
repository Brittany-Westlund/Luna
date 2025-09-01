using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    public Transform holdPoint;
    public GameObject currentFlowerObject;
    public string currentFlowerType;

    [Header("Held Flower Prefabs")]
    public List<FlowerPrefabEntry> flowerPrefabs;
    private Dictionary<string, FlowerPrefabEntry> flowerPrefabMap;

    [Header("Plantable Flower Prefabs")]
    public GameObject TeaRose;
    public GameObject Goldenrod;
    public GameObject Buttercup;
    public GameObject Foxglove;
    public GameObject Iris;
    public GameObject Moonflower;
    public GameObject Anemone;
    // Add all other flower prefabs here

    private Dictionary<string, GameObject> plantedFlowerPrefabs;

    [System.Serializable]
    public class FlowerPrefabEntry
    {
        public string flowerType;
        public GameObject heldPrefab;
        public Vector3 offset;
    }

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

        plantedFlowerPrefabs = new Dictionary<string, GameObject>
        {
            { "TeaRose", TeaRose },
            { "Goldenrod", Goldenrod },
            { "Buttercup", Buttercup },
            { "Anemone", Anemone },
            { "Iris", Iris },
            { "Foxglove", Foxglove },
             { "Moonflower", Moonflower }
            // Add more mappings here
        };
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
            currentFlowerType = null;
        }
    }

    public void PlantFlower(Vector3 position)
    {
        if (!HasFlower()) return;

        if (plantedFlowerPrefabs.TryGetValue(currentFlowerType, out GameObject prefab))
        {
            Instantiate(prefab, position, Quaternion.identity);
            Debug.Log("🌿 Planted flower: " + currentFlowerType + " at " + position);
        }
        else
        {
            Debug.LogWarning("❌ No prefab found to plant for flower type: " + currentFlowerType);
        }

        DropFlower();
    }
}
