using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    public Transform holdPoint;
    private GameObject heldFlower;
    private List<GardenSpot> gardensInRange = new List<GardenSpot>();
    private List<GameObject> flowersInRange = new List<GameObject>();

    public bool HasFlower => heldFlower != null;

    void Update()
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
        foreach (var flower in flowersInRange)
        {
            if (flower == null) continue;

            var sprout = flower.GetComponent<SproutAndLightManager>();
            if (sprout == null || sprout.isHeld) continue;

            // If planted, remove from garden first
            if (sprout.isPlanted)
            {
                GardenSpot garden = flower.transform.parent?.GetComponent<GardenSpot>();
                if (garden != null && garden.HasPlantedFlower)
                {
                    garden.PickUp(); // clears and sets flags
                }
            }

            PickUpFlower(flower);
            Debug.Log($"🌸 Picked up {flower.name}");
            return;
        }
    }

    private void TryPlantFlower()
    {
        foreach (var garden in gardensInRange)
        {
            if (garden.HasPlantedFlower)
            {
                GameObject old = garden.PickUp();
                if (old != null) PickUpFlower(old);
            }

            garden.Plant(heldFlower, garden.GetPlantingPoint().position);
            DropFlower();
            return;
        }
    }

    public void PickUpFlower(GameObject flower)
    {
        if (flower == null) return;

        heldFlower = flower;
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
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sprout"))
        {
            if (!flowersInRange.Contains(other.gameObject))
                flowersInRange.Add(other.gameObject);
        }
        else if (other.CompareTag("Garden"))
        {
            GardenSpot g = other.GetComponent<GardenSpot>();
            if (g != null && !gardensInRange.Contains(g))
            {
                gardensInRange.Add(g);
                g.SetHighlight(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Sprout"))
        {
            flowersInRange.Remove(other.gameObject);
        }
        else if (other.CompareTag("Garden"))
        {
            GardenSpot g = other.GetComponent<GardenSpot>();
            if (g != null && gardensInRange.Contains(g))
            {
                gardensInRange.Remove(g);
                g.SetHighlight(false);
            }
        }
    }
}
