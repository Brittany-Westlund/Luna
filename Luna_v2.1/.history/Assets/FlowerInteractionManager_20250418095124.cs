using UnityEngine;
using System.Collections.Generic;

public class FlowerInteraction : MonoBehaviour
{
    public Transform holdPoint;
    private GameObject heldFlower;
    private readonly List<GameObject> nearbyFlowers = new();
    private readonly List<GardenSpot> nearbyGardens = new();

    void Update()
    {
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
        if (closest == null)
            return;

        var sprout = closest.GetComponent<SproutAndLightManager>();
        if (sprout == null)
            return;

        // If it's planted, clear it from its garden
        if (sprout.isPlanted && closest.transform.parent != null)
        {
            var garden = closest.transform.parent.GetComponent<GardenSpot>();
            if (garden != null)
                garden.ClearPlantedFlower();
        }

        // Pick it up
        sprout.isHeld = true;
        sprout.isPlanted = false;

        closest.transform.SetParent(holdPoint);
        closest.transform.localPosition = Vector3.zero;

        var col = closest.GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        heldFlower = closest;
    }

    private void TryPlantFlower()
    {
        if (heldFlower == null)
            return;

        GardenSpot garden = FindClosestGarden();
        if (garden == null)
            return;

        // Cache original and any existing flower
        GameObject original = heldFlower;
        GameObject swapped = garden.GetPlantedFlower();

        // 1) Plant the original flower
        original.transform.SetParent(garden.transform);
        original.transform.position = garden.GetPlantingPoint().position;

        var origSprout = original.GetComponent<SproutAndLightManager>();
        if (origSprout != null)
        {
            origSprout.isPlanted = true;
            origSprout.isHeld = false;
        }
        var origCol = original.GetComponent<Collider2D>();
        if (origCol != null)
            origCol.enabled = true;

        garden.SetPlantedFlower(original);

        // 2) If there was a flower in the garden, pick it up
        if (swapped != null)
        {
            garden.ClearPlantedFlower();

            var swapSprout = swapped.GetComponent<SproutAndLightManager>();
            if (swapSprout != null)
            {
                swapSprout.isPlanted = false;
                swapSprout.isHeld = true;
            }

            swapped.transform.SetParent(holdPoint);
            swapped.transform.localPosition = Vector3.zero;

            var swapCol = swapped.GetComponent<Collider2D>();
            if (swapCol != null)
                swapCol.enabled = false;

            heldFlower = swapped;
        }
        else
        {
            // No swap: you're now empty-handed
            heldFlower = null;
        }
    }

    private GameObject FindClosestFlower()
    {
        GameObject closest = null;
        float minDist = float.MaxValue;

        foreach (var flower in nearbyFlowers)
        {
            if (flower == null || flower == heldFlower)
                continue;

            float dist = Vector2.Distance(transform.position, flower.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = flower;
            }
        }

        return closest;
    }

    private GardenSpot FindClosestGarden()
    {
        GardenSpot closest = null;
        float minDist = float.MaxValue;

        foreach (var g in nearbyGardens)
        {
            if (g == null)
                continue;

            float dist = Vector2.Distance(transform.position, g.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = g;
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
            {
                nearbyGardens.Add(g);
                g.SetHighlight(true);
            }
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
                nearbyGardens.Remove(g);
                g.SetHighlight(false);
            }
        }
    }
}
