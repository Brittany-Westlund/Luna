using UnityEngine;
using System.Collections.Generic;

public class FlowerInteraction : MonoBehaviour
{
    public Transform holdPoint;
    [Tooltip("Radius around garden to detect pre-placed flowers for swapping.")]
    public float swapDetectRadius = 1f;

    private GameObject heldFlower;
    private GardenSpot currentGarden;
    private readonly List<GameObject> nearbyFlowers = new();
    private readonly List<GardenSpot> nearbyGardens = new();

    void Update()
    {
        // Only highlight the closest garden
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

        // 1) Look for any previously planted flower
        GameObject swapped = garden.GetPlantedFlower();

        // 1a) Check children under this garden
        if (swapped == null)
        {
            foreach (Transform child in garden.transform)
            {
                if (child.TryGetComponent<SproutAndLightManager>(out var mgr) && mgr.isPlanted)
                {
                    swapped = child.gameObject;
                    break;
                }
            }
        }

        // 1b) If still none, do a proximity check
        if (swapped == null)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(garden.transform.position, swapDetectRadius);
            foreach (var hit in hits)
            {
                if (hit.TryGetComponent<SproutAndLightManager>(out var mgr) && mgr.isPlanted)
                {
                    swapped = hit.gameObject;
                    break;
                }
            }
        }

        // 2) Clear old record
        garden.ClearPlantedFlower();

        // 3) Plant the held flower
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

        // 4) Swap back any pre-existing flower
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

        // Find the nearest garden
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
