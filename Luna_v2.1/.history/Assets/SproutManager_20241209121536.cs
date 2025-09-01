using System.Collections.Generic;
using UnityEngine;

public class SproutManager : MonoBehaviour
{
    private List<GameObject> flowers = new List<GameObject>();

    // Register a new flower
    public void RegisterFlower(GameObject flower)
    {
        if (!flowers.Contains(flower))
        {
            flowers.Add(flower);
            Debug.Log($"Flower {flower.name} registered.");
        }
    }

    // Unregister a flower
    public void UnregisterFlower(GameObject flower)
    {
        if (flowers.Contains(flower))
        {
            flowers.Remove(flower);
            Debug.Log($"Flower {flower.name} unregistered.");
        }
    }

    // Try to aid a sprout
    public bool TryAidSprout(GameObject flower)
    {
        SproutAndLightManager sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.ResetOnGrowth();
            return true;
        }
        Debug.LogWarning($"Cannot aid flower {flower.name} as it lacks a SproutAndLightManager.");
        return false;
    }
}
