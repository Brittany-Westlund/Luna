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
            Debug.Log($"Flower {flower.name} registered with SproutManager.");
        }
    }

    public void UnregisterFlower(GameObject flower)
    {
        if (flowers.Contains(flower))
        {
            flowers.Remove(flower);
            Debug.Log($"Flower {flower.name} unregistered from SproutManager.");
        }
    }

    public void ActivateFlower(GameObject flower)
    {
        FlowerBehavior flowerBehavior = flower.GetComponent<FlowerBehavior>();
        if (flowerBehavior != null)
        {
            if (flowerBehavior.IsFullyGrown())
            {
                flowerBehavior.ActivateLight();
                Debug.Log($"Flower {flower.name} activated.");
            }
            else
            {
                Debug.Log($"Cannot activate flower {flower.name} as it is not fully grown.");
            }
        }
        else
        {
            Debug.LogWarning($"Flower {flower.name} does not have a FlowerBehavior component.");
        }
    }

    public bool TryAidSprout(GameObject flower)
    {
        SproutAndLightManager sproutManager = flower.GetComponent<SproutAndLightManager>();
        if (sproutManager != null)
        {
            if (!sproutManager.IsFullyGrown())
            {
                sproutManager.ResetOnGrowth();
                return true;
            }
        }
        else
        {
            Debug.LogWarning($"Flower {flower.name} does not have a SproutAndLightManager component.");
        }
        return false;
    }
}
