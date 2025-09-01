using UnityEngine;
using System.Collections.Generic;

public class FlowerInteraction : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Transform holdPoint;

    [Header("Teapot Hookup (so X goes to teapot when close)")]
    public TeapotReceiver teapotReceiver;  // Drag your TeapotReceiver here in the Inspector

    private GameObject heldFlower;
    private GardenSpot currentGarden;
    private readonly List<GameObject> nearbyFlowers = new();
    private readonly List<GardenSpot> nearbyGardens = new();

    void Update()
    {
        // If I'm holding a flower AND the player is in range of the teapot,
        // bail out here so the TeapotReceiver gets the X press instead.
        if (heldFlower != null 
            && teapotReceiver != null 
            && teapotReceiver.PlayerIsNearby)    // TeapotReceiver must expose a public bool PlayerIsNearby
        {
            return;
        }

        // Otherwise, handle garden‐pickup/plant
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

        sprout.isHeld    = true;
        sprout.isPlanted = false;
        heldFlower       = closest;

        closest.transform.SetParent(holdPoint);
        closest.transform.localPosition = Vector3.zero;
        if (closest.TryGetComponent<Collider2D>(out var col))
            col.enabled = false;
    }

    private void TryPlantFlower()
    {
        if (heldFlower == null || currentGarden == null) return;
        GardenSpot garden = currentGarden;

        // find any flower already under this garden
        GameObject swapped = garden.GetPlantedFlower();
        if (swapped == null)
        {
            foreach (Transform child in garden.transform)
            {
                if (child.gameObject != heldFlower 
                    && child.TryGetComponent<SproutAndLightManager>(out _))
                {
                    swapped = child.gameObject;
                    break;
                }
            }
        }

        garden.ClearPlantedFlower();

        // place heldFlower into garden
        heldFlower.transform.SetParent(garden.transform);
        heldFlower.transform.position = garden.GetPlantingPoint().position;
        if (heldFlower.TryGetComponent<SproutAndLightManager>(out var origSprout))
        {
            origSprout.isPlanted = true;
            origSprout.isHeld    = false;
        }
        if (heldFlower.TryGetComponent<Collider2D>(out var origCol))
            origCol.enabled = true;

        garden.SetPlantedFlower(heldFlower);

        // swap back any previous flower
        if (swapped != null)
        {
            if (swapped.TryGetComponent<SproutAndLightManager>(out var swapSprout))
            {
                swapSprout.isPlanted = false;
                swapSprout.isHeld    = true;
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
            if (!flower.TryGetComponent<SproutAndLightManager>(out _)) continue;

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
        if (other.CompareTag("Sprout") && other.TryGetComponent<SproutAndLightManager>(out _))
        {
            if (!nearbyFlowers.Contains(other.gameObject))
                nearbyFlowers.Add(other.gameObject);
        }
        else if (other.CompareTag("Garden"))
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
        else if (other.CompareTag("Garden"))
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
