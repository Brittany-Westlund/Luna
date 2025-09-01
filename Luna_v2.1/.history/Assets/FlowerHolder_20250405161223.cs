using System.Collections.Generic;
using UnityEngine;

public class FlowerHolder : MonoBehaviour
{
    public Transform holdPoint;
    public GameObject currentFlowerObject;
    public string currentFlowerType;

    [Header("Held Flower Prefabs")]
    public List<FlowerPrefabEntry> flowerPrefabs;

    private Dictionary<string, GameObject> flowerPrefabMap;

    [System.Serializable]
    public class FlowerPrefabEntry
    {
        public string flowerType;
        public GameObject heldPrefab;
    }

    private void Start()
    {
        flowerPrefabMap = new Dictionary<string, GameObject>();
        foreach (var entry in flowerPrefabs)
        {
            if (!flowerPrefabMap.ContainsKey(entry.flowerType))
            {
                flowerPrefabMap.Add(entry.flowerType, entry.heldPrefab);
            }
        }
    }

    public bool HasFlower() => currentFlowerObject != null;

    public void PickUpFlower(FlowerPickup flower)
    {
        currentFlowerType = flower.flowerType;

        if (flowerPrefabMap.TryGetValue(currentFlowerType, out GameObject heldPrefab))
        {
            currentFlowerObject = Instantiate(heldPrefab, holdPoint.position, Quaternion.identity, holdPoint);
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
}
