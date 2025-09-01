using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    public Transform holdPoint;

    private GameObject heldFlower = null;
    private GardenSpot currentGarden = null;
    private FlowerPickup currentFlowerTarget = null;

    public bool HasFlower => heldFlower != null;
    public GameObject GetHeldFlower() => heldFlower;
    public string CurrentFlowerType => heldFlower != null ? heldFlower.GetComponent<FlowerPickup>()?.flowerType : "Unknown";

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (HasFlower && currentGarden != null)
            {
                // Plant or swap flowers
                if (currentGarden.HasPlantedFlower)
                {
                    GameObject previousFlower = currentGarden.PickUp();
                    currentGarden.Plant(heldFlower, currentGarden.GetPlantingPoint().position);
                    PickUpFlower(previousFlower);
                }
                else
                {
                    currentGarden.Plant(heldFlower, currentGarden.GetPlantingPoint().position);
                    DropFlower();
                }
            }
            else if (!HasFlower && currentFlowerTarget != null)
            {
                PickUpFlower(currentFlowerTarget.gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            GardenSpot g = other.GetComponent<GardenSpot>();
            if (g != null)
            {
                currentGarden = g;
                currentGarden.SetHighlight(true);
            }
        }

        if (other.CompareTag("Sprout"))
        {
            FlowerPickup f = other.GetComponent<FlowerPickup>();
            if (f != null && !HasFlower)
            {
                currentFlowerTarget = f;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            GardenSpot g = other.GetComponent<GardenSpot>();
            if (g != null && g == currentGarden)
            {
                currentGarden.SetHighlight(false);
                currentGarden = null;
            }
        }

        if (other.CompareTag("Sprout"))
        {
            FlowerPickup f = other.GetComponent<FlowerPickup>();
            if (f != null && f == currentFlowerTarget)
            {
                currentFlowerTarget = null;
            }
        }
    }

    public void PickUpFlower(GameObject flower)
    {
        if (flower == null) return;

        heldFlower = flower;
        flower.transform.SetParent(holdPoint);
        flower.transform.localPosition = Vector3.zero;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = true;
            sprout.isPlanted = false;
        }

        var col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    public void DropFlower()
    {
        heldFlower = null;
    }
}
