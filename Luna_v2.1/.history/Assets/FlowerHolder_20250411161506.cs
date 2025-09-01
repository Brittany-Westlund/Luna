using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    public Transform holdPoint;
    public GameObject currentFlowerObject;
    public string currentFlowerType;
    public Dictionary<string, GameObject> flowerPrefabs = new Dictionary<string, GameObject>();

    [Header("Held Flower Prefabs")]
    public List<FlowerPrefabEntry> flowerPrefabs;

    private Dictionary<string, FlowerPrefabEntry> flowerPrefabMap;

    [System.Serializable]
    public class FlowerPrefabEntry
    {
        public string flowerType;
        public GameObject heldPrefab;
        public Vector3 offset; // Custom offset for positioning
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

    public void PlantFlower(Vector3 position, string flowerType)
    {
        if (!HasFlower()) return;

        GameObject planted = Instantiate(flowerPrefab, position, Quaternion.identity); // flowerPrefab must match type!
        currentFlowerType = null;
        Debug.Log("🌿 Flower planted at " + position);
    }

}
