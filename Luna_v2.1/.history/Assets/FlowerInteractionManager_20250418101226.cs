using UnityEngine;
using System.Collections.Generic;

public class FlowerInteraction : MonoBehaviour
{
    public Transform holdPoint;

    private GameObject heldFlower;
    private GardenSpot currentGarden;
    private readonly List<GameObject> nearbyFlowers = new();
    private readonly List<GardenSpot> nearbyGardens = new();

    void Update()
    {
        UpdateGardenHighlight();

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (heldFlower == null)
                TryPickUpFlower();
            else
                TryPlantFlower();
        }
    }

    private void TryPickUpFlower()
    {
        GameObject closest = FindClosestFlower();
        if (closest == null) return;

        var sprout = closest.GetComponent<SproutAndLightManager>();
        if (sprout == null) return;

        if (sprout.isPlanted && closest.transform.parent != null)
        {
            var garden = closest.transform.parent.GetComponent<GardenSpot>();
            if (garden != null)
                garden.ClearPlantedFlower();
        }

        sprout.isHeld = true;
        sprout.isPlanted = false;

        closest.transform.SetParent(holdPoint);
        closest.transform.localPosition = Vector3.zero;

        if (closest.TryGetComponent<Collider2D>(out var col))
            col.enabled = false;

        heldFlower = closest;
    }

    private void TryPlantFlower()
    {
        if (heldFlower == null || currentGarden == null) return;
        GardenSpot garden = currentGarden;

        // 1) Get any flower currently in the garden
        GameObject swapped = garden.GetPlantedFlower();

        // 1a) Fallback to any direct child (pre-placed)
        if (swapped == null)
        {
            foreach (Transform child in garden.transform)
            {
                if (child.gameObject != heldFlower)
                {
                    swapped = child.gameObject;
                    break;
                }
            }
        }

        // 2) Clear GardenSpot record and detach swapped
        garden.ClearPlantedFlower();

        // 3) Plant heldFlower
        GameObject original = heldFlower;
        original.transform.SetParent(garden.transform);
        original.transform.position = garden.GetPlantingPoint().position;

        if (original.TryGetComponent<SproutAndLightManager>(out var origSprout))
        {
            origSprout.isPlanted = true;
            origSprout.isHeld = false;
        }
        if (original.TryGetComponent<Collider2D>(out var origCol))
            origCol.enabled = true;

        garden.SetPlantedFlower(original);

        // 4) Swap back any existing child
        if (swapped != null && swapped != original)
        {
            if (swapped.TryGetComponent<SproutAndLightManager>(out var swapSprout))
            {
                swapSprout.isPlanted = false;
                swapSprout.isHeld = true;
            }

            swapped.transform.SetParent(holdPoint);
            swapped.transform.localPosition = Vector3.zero;

            if (swapped.TryGetComponent<Collider2D>(out var swapCol))
                swapCol.enabled = false;

            heldFlower = swapped;
        }
        else
        {
            heldFlower = null;
        }
    }

    private void UpdateGardenHighlight()
    {
        GardenSpot closest = null;
        float minDist = float.MaxValue;

        foreach (var g in nearbyGardens)
        {
            if (g == null) continue;
            float d = Vector2.Distance(transform.position, g.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = g;
            }
        }

        if (closest != currentGarden)
        {
            if (currentGarden != null)
                currentGarden.SetHighlight(false);
            currentGarden = closest;
            if (currentGarden != null)
                currentGarden.SetHighlight(true);
        }
    }

    private GameObject FindClosestFlower()
    {
        GameObject closest = null;
        float minDist = float.MaxValue;

        foreach (var flower in nearbyFlowers)
        {
            if (flower == null || flower == heldFlower) continue;
            float d = Vector2.Distance(transform.position, flower.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closest = flower;
            }
        }
        return closest;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sprout") && !nearbyFlowers.Contains(other.gameObject))
            nearbyFlowers.Add(other.gameObject);

        if (other.CompareTag("Garden"))
        {
            var g = other.GetComponent<GardenSpot>();
            if (g != null && !nearbyGardens.Contains(g))
                nearbyGardens.Add(g);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Sprout"))
            nearbyFlowers.Remove(other.gameObject);

        if (other.CompareTag("Garden"))
        {
            var g = other.GetComponent<GardenSpot>();
            if (g != null && nearbyGardens.Contains(g))
            {
                if (currentGarden == g)
                {
                    g.SetHighlight(false);
                    currentGarden = null;
                }
                nearbyGardens.Remove(g);
            }
        }
    }
}
