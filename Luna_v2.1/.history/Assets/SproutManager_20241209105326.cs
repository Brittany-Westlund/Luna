using System.Collections.Generic;
using UnityEngine;

public class SproutManager : MonoBehaviour
{
    private List<GameObject> flowers = new List<GameObject>();

    public void RegisterFlower(GameObject flower)
    {
        if (!flowers.Contains(flower))
        {
            flowers.Add(flower);
            Debug.Log($"Flower registered: {flower.name}");
        }
    }

    public void UnregisterFlower(GameObject flower)
    {
        if (flowers.Contains(flower))
        {
            flowers.Remove(flower);
            Debug.Log($"Flower unregistered: {flower.name}");
        }
    }

    public void ActivateFlower(GameObject flower)
    {
        // Get the SproutAndLightManager component from the flower
        SproutAndLightManager lightManager = flower.GetComponent<SproutAndLightManager>();
        if (lightManager != null)
        {
            if (lightManager.IsFullyGrown())
            {
                lightManager.ActivateLight();
                Debug.Log($"Activated light for flower: {flower.name}");
            }
            else
            {
                Debug.Log($"Cannot activate flower {flower.name} as it is not fully grown.");
            }
        }
        else
        {
            Debug.LogWarning($"Flower {flower.name} is missing SproutAndLightManager!");
        }
    }

    public bool TryAidSprout(GameObject sprout)
    {
        SproutAndLightManager lightManager = sprout.GetComponent<SproutAndLightManager>();
        if (lightManager != null)
        {
            if (!lightManager.IsFullyGrown())
            {
                lightManager.ResetOnGrowth();
                Debug.Log($"Aided sprout: {sprout.name}");
                return true; // Successful aid
            }
            else
            {
                Debug.Log($"Sprout {sprout.name} is already fully grown.");
            }
        }
        else
        {
            Debug.LogWarning($"Sprout {sprout.name} is missing SproutAndLightManager!");
        }

        return false; // Aid not successful
    }
}
