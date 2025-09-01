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
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (heldFlower == null)
            {
                TryPickUpFlower();
            }
            else
            {
                TryPlantFlower();
            }
        }
    }

    private void TryPickUpFlower()
    {
        GameObject closest = FindClosestFlower();
        if (closest == null) return;

        var sprout = closest.GetComponent<SproutAndLightManager>();
        if (sprout == null) return;

        // Remove it from the garden if it's planted
        if (sprout.isPlanted && closest.transform.parent != null)
        {
            var garden = closest.transform.parent.GetComponent<GardenSpot>();
            if (garden != null)
            {
                garden.ClearPlantedFlower();
            }
        }

        sprout.isHeld = true;
        sprout.isPlanted = false;

        closest.transform.SetParent(holdPoint);
        closest.transform.localPosition = Vector3.zero;

        var col = closest.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        heldFlower = closest;
    }

    private void TryPlantFlower()
    {
        GardenSpot garden = FindClosestGarden();
        if (garden == null || heldFlower == null) return;

        GameObject swappedFlower = garden.GetPlantedFlower();
        if (swappedFlower != null)
        {
            PickUpNewFlower(swappedFlower);
        }

        heldFlower.transform.SetParent(garden.transform);
        heldFlower.transform.position = garden.GetPlantingPoint().position;

        var sprout = heldFlower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isPlanted = true;
            sprout.isHeld = false;
        }

        var col = heldFlower.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        garden.SetPlantedFlower(heldFlower);
        heldFlower = null;
    }

    private void PickUpNewFlower(GameObject flower)
    {
        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = true;
            sprout.isPlanted = false;
        }

        flower.transform.SetParent(holdPoint);
        flower.transform.localPosition = Vector3.zero;

        var col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        heldFlower = flower;
    }

    private GameObject FindClosestFlower()
    {
        GameObject closest = null;
        float minDist = float.MaxValue;

        foreach (var flower in nearbyFlowers)
        {
            if (flower == null) continue;
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
            if (g == null) continue;
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
        {
            nearbyFlowers.Add(other.gameObject);
        }

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
        {
            nearbyFlowers.Remove(other.gameObject);
        }

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
