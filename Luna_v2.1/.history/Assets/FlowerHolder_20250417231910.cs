using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    public Transform holdPoint;

    private GameObject heldFlower;
    private string heldFlowerType = "";
    private readonly List<GardenSpot> nearbyGardens = new();

    public bool HasFlower => heldFlower != null;
    public GameObject GetHeldFlower() => heldFlower;
    public string CurrentFlowerType => heldFlowerType;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (HasFlower)
            {
                TryPlantFlower();
            }
            else
            {
                TryPickUpCollidingFlower();
            }
        }
    }

    private void TryPickUpCollidingFlower()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Sprout"))
            {
                var sprout = hit.GetComponent<SproutAndLightManager>();
                if (sprout == null || sprout.isHeld) continue;

                PickUpFlower(hit.gameObject);
                break;
            }
        }
    }

    private void TryPlantFlower()
    {
        GardenSpot closest = FindClosestGarden();
        if (closest == null) return;

        var sprout = heldFlower.GetComponent<SproutAndLightManager>();
        if (sprout != null && !sprout.isPlanted)
        {
            GameObject flowerToSwap = closest.HasPlantedFlower ? closest.PickUp() : null;
            closest.Plant(heldFlower, closest.GetPlantingPoint().position);
            DropFlower();

            if (flowerToSwap != null)
            {
                PickUpFlower(flowerToSwap);
            }
        }
    }

    public void PickUpFlower(GameObject flower)
    {
        if (flower == null) return;

        heldFlower = flower;
        heldFlowerType = flower.GetComponent<FlowerPickup>()?.flowerType ?? "Unknown";

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

        flower.SetActive(true);
    }

    public void DropFlower()
    {
        heldFlower = null;
        heldFlowerType = "";
    }

    private GardenSpot FindClosestGarden()
    {
        GardenSpot closest = null;
        float minDist = float.MaxValue;

        foreach (var g in nearbyGardens)
        {
            float dist = Vector3.Distance(transform.position, g.transform.position);
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
        if (other.CompareTag("Garden"))
        {
            GardenSpot g = other.GetComponent<GardenSpot>();
            if (g != null && !nearbyGardens.Contains(g))
            {
                nearbyGardens.Add(g);
                g.SetHighlight(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            GardenSpot g = other.GetComponent<GardenSpot>();
            if (g != null && nearbyGardens.Contains(g))
            {
                nearbyGardens.Remove(g);
                g.SetHighlight(false);
            }
        }
    }
}
