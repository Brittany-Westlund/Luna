/* using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    public Transform holdPoint;
    private GameObject heldFlower;
    private List<GardenSpot> nearbyGardens = new List<GardenSpot>();

    public bool HasFlower => heldFlower != null;
    public GameObject GetHeldFlower() => heldFlower;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X) && HasFlower)
        {
            TryPlantFlower();
        }
    }

    public void PickUpFlower(GameObject flower)
    {
        if (HasFlower) return;

        var pickup = flower.GetComponent<FlowerPickup>();
        if (pickup != null) pickup.isPickedUp = true;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = true;
            sprout.isPlanted = false;
        }

        heldFlower = flower;
        flower.transform.SetParent(holdPoint);
        flower.transform.localPosition = Vector3.zero;

        Collider2D col = flower.GetComponent<Collider2D>();
        if (col) col.enabled = false;
    }

    private void TryPlantFlower()
    {
        GardenSpot closest = FindClosestGarden();
        if (closest == null) return;

        GameObject previous = closest.PickUp(); // Picks up old flower (if any)
        closest.Plant(heldFlower, closest.GetPlantingPoint().position);

        if (previous != null)
        {
            PickUpFlower(previous); // Automatically picks it up
        }
        else
        {
            DropFlower(); // If nothing to swap, just drops held
        }
    }

    public void DropFlower()
    {
        if (heldFlower != null)
        {
            var pickup = heldFlower.GetComponent<FlowerPickup>();
            if (pickup != null) pickup.isPickedUp = false;

            heldFlower.transform.SetParent(null);
            Collider2D col = heldFlower.GetComponent<Collider2D>();
            if (col) col.enabled = true;
        }

        heldFlower = null;
    }

    private GardenSpot FindClosestGarden()
    {
        GardenSpot closest = null;
        float minDist = float.MaxValue;

        foreach (GardenSpot g in nearbyGardens)
        {
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

    public string CurrentFlowerType
    {
        get
        {
            var pickup = heldFlower?.GetComponent<FlowerPickup>();
            return pickup != null ? pickup.flowerType : "Unknown";
        }
    }

} */
