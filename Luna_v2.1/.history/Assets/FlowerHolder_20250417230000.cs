using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    public Transform holdPoint;
    private GameObject heldFlower;
    private string heldFlowerType = "";
    private List<GardenSpot> nearbyGardens = new List<GardenSpot>();

    public bool HasFlower => heldFlower != null;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (HasFlower)
                TryPlantFlower();
        }
    }

    public void PickUpFlower(GameObject flower)
    {
        if (HasFlower)
        {
            Debug.LogWarning("Already holding a flower!");
            return;
        }

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

    private void TryPlantFlower()
    {
        GardenSpot closest = FindClosestGarden();
        if (closest == null || !IsTouching(closest)) return;

        var sprout = heldFlower.GetComponent<SproutAndLightManager>();
        if (sprout != null && !sprout.isPlanted)
        {
            if (closest.HasPlantedFlower)
            {
                GameObject oldFlower = closest.PickUp();
                if (oldFlower != null)
                {
                    DropFlower(); // drop current
                    PickUpFlower(oldFlower); // pick up replaced
                }
            }

            closest.Plant(heldFlower, closest.GetPlantingPoint().position);
            DropFlower();
        }
    }

    private bool IsTouching(GardenSpot garden)
    {
        Collider2D myCollider = GetComponent<Collider2D>();
        Collider2D gardenCollider = garden.GetComponent<Collider2D>();
        return myCollider != null && gardenCollider != null && myCollider.IsTouching(gardenCollider);
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
            if (g != null)
            {
                g.SetHighlight(false);
                nearbyGardens.Remove(g);
            }
        }
    }
}
