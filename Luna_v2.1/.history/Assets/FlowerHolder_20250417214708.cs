using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    public Transform holdPoint;

    private GameObject heldFlower;
    private string heldFlowerType = "";
    private List<GardenSpot> nearbyGardens = new List<GardenSpot>();

    public bool HasFlower => heldFlower != null;
    public string CurrentFlowerType => heldFlowerType;
    public GameObject GetHeldFlower() => heldFlower;

   private void Update()
    {
        // If the player is holding a flower, check for planting
        if (Input.GetKeyDown(KeyCode.X) && HasFlower)
        {
            GardenSpot closest = FindClosestGarden();
            if (closest == null) return;

            var sprout = heldFlower.GetComponent<SproutAndLightManager>();
            if (sprout != null && !sprout.isPlanted)
            {
                if (closest.HasPlantedFlower)
                    closest.PickUp();

                closest.Plant(heldFlower, closest.GetPlantingPoint().position);
                DropFlower();
            }
        }

        // Only try to pick up from garden if not holding a flower
        else if (Input.GetKeyDown(KeyCode.X) && !HasFlower)
        {
            // Check if there's any loose flower in range — let FlowerPickup handle it
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Flower"))
                {
                    var sprout = hit.GetComponent<SproutAndLightManager>();
                    if (sprout != null && !sprout.isPlanted)
                    {
                        // Loose flower nearby — abort GardenSpot logic
                        return;
                    }
                }
            }

            // Proceed with garden pickup
            GardenSpot closest = FindClosestGarden();
            if (closest != null && closest.HasPlantedFlower)
            {
                GameObject flower = closest.PickUp();
                if (flower != null)
                {
                    Debug.Log($"🌱 Picking up planted flower: {flower.name}");
                    PickUpFlower(flower);
                }
            }
        }
    }

    public void PickUpFlowerInstance(GameObject flower)
    {
        if (!HasFlower && flower != null)
        {
            PickUpFlower(flower);
        }
    }

    private void PickUpFlower(GameObject flower)
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
