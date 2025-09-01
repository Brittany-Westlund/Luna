using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    public Transform holdPoint;

    private GameObject heldFlower;
    private string heldFlowerType = "";
    private GardenSpot nearbyGarden;
    private GameObject flowerInRange;

    public bool HasFlower => heldFlower != null;
    public string CurrentFlowerType => heldFlowerType;
    public GameObject GetHeldFlower() => heldFlower;

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
                TryPickUpFlower();
            }
        }
    }

    private void TryPickUpFlower()
    {
        if (flowerInRange == null) return;

        var sprout = flowerInRange.GetComponent<SproutAndLightManager>();
        if (sprout == null) return;

        heldFlower = flowerInRange;
        heldFlowerType = heldFlower.GetComponent<FlowerPickup>()?.flowerType ?? "Unknown";

        sprout.isHeld = true;
        sprout.isPlanted = false;

        flowerInRange.transform.SetParent(holdPoint);
        flowerInRange.transform.localPosition = Vector3.zero;

        var col = flowerInRange.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    private void TryPlantFlower()
    {
        if (nearbyGarden == null) return;

        var sprout = heldFlower.GetComponent<SproutAndLightManager>();
        if (sprout == null || sprout.isPlanted) return;

        if (nearbyGarden.HasPlantedFlower)
        {
            GameObject swapped = nearbyGarden.PickUp();
            PickUpFlower(swapped);
        }

        nearbyGarden.Plant(heldFlower, nearbyGarden.GetPlantingPoint().position);
        DropFlower();
    }

    public void PickUpFlower(GameObject flower)
    {
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
    }

    public void DropFlower()
    {
        heldFlower = null;
        heldFlowerType = "";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            var g = other.GetComponent<GardenSpot>();
            if (g != null)
            {
                nearbyGarden = g;
                g.SetHighlight(true);
            }
        }

        if (other.CompareTag("Sprout"))
        {
            flowerInRange = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            var g = other.GetComponent<GardenSpot>();
            if (g != null && g == nearbyGarden)
            {
                g.SetHighlight(false);
                nearbyGarden = null;
            }
        }

        if (other.CompareTag("Sprout") && flowerInRange == other.gameObject)
        {
            flowerInRange = null;
        }
    }
}
