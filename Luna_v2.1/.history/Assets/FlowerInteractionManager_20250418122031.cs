using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(FlowerHolder))]
public class FlowerInteractionManager : MonoBehaviour
{
    [Header("Hold‑Point (shared with FlowerHolder)")]
    public Transform holdPoint;

    [Header("Auto‑detect TeapotReceiver")]
    public TeapotReceiver teapotReceiver;

    private FlowerHolder flowerHolder;
    private GardenSpot currentGarden;
    private readonly List<GameObject> nearbyFlowers = new List<GameObject>();
    private readonly List<GardenSpot>  nearbyGardens  = new List<GardenSpot>();

    void Awake()
    {
        flowerHolder = GetComponent<FlowerHolder>();
        flowerHolder.holdPoint = holdPoint;
    }

    void Update()
    {
        bool holding = flowerHolder.HasFlower;

        // 1) Teapot has priority
        if (teapotReceiver != null && teapotReceiver.PlayerIsNearby)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                if (holding)
                    teapotReceiver.AddFlowerToTeapot(flowerHolder);
                else if (teapotReceiver.HasAnyIngredients())
                    teapotReceiver.RetrieveLastFlower(flowerHolder);
            }
            return;
        }

        // 2) Garden pickup/plant
        HighlightClosestGarden();
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (!holding) TryPickUpFromGarden();
            else         TryPlantToGarden();
        }
    }

    private void TryPickUpFromGarden()
    {
        GameObject closest = FindClosestFlower();
        if (closest == null) return;

        var sprout = closest.GetComponent<SproutAndLightManager>();
        if (sprout == null) return;

        // Clear previous garden spot
        if (sprout.isPlanted && closest.transform.parent != null)
        {
            var oldSpot = closest.transform.parent.GetComponent<GardenSpot>();
            if (oldSpot != null) oldSpot.ClearPlantedFlower();
        }

        // Pickup and remove from list immediately
        flowerHolder.PickUpFlower(closest);
        nearbyFlowers.Remove(closest);
    }

    private void TryPlantToGarden()
    {
        if (!flowerHolder.HasFlower || currentGarden == null) return;

        var held    = flowerHolder.GetHeldFlower();
        GameObject swapped = currentGarden.GetPlantedFlower();

        // If garden had no tracked flower, check its children
        if (swapped == null)
        {
            foreach (Transform child in currentGarden.transform)
            {
                if (child.gameObject != held &&
                    child.GetComponent<SproutAndLightManager>() != null)
                {
                    swapped = child.gameObject;
                    break;
                }
            }
        }

        currentGarden.ClearPlantedFlower();

        // Plant held
        held.transform.SetParent(currentGarden.transform);
        held.transform.position = currentGarden.GetPlantingPoint().position;
        var sm = held.GetComponent<SproutAndLightManager>();
        if (sm != null) { sm.isPlanted = true; sm.isHeld = false; }
        var col = held.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
        currentGarden.SetPlantedFlower(held);
        flowerHolder.DropFlower();

        // If we swapped one out, re‑add it to nearbyFlowers so you can grab it immediately
        if (swapped != null)
        {
            flowerHolder.PickUpFlower(swapped);
            if (!nearbyFlowers.Contains(swapped))
                nearbyFlowers.Add(swapped);
        }
    }

    private void HighlightClosestGarden()
    {
        GardenSpot best = null;
        float md = float.MaxValue;
        foreach (var g in nearbyGardens)
        {
            if (g == null) continue;
            float d = Vector2.Distance(transform.position, g.transform.position);
            if (d < md) { md = d; best = g; }
        }

        if (best != currentGarden)
        {
            currentGarden?.SetHighlight(false);
            currentGarden = best;
            currentGarden?.SetHighlight(true);
        }
    }

    private GameObject FindClosestFlower()
    {
        GameObject best = null;
        float md = float.MaxValue;
        foreach (var f in nearbyFlowers)
        {
            if (f == null) continue;
            if (f.GetComponent<SproutAndLightManager>() == null) continue;
            float d = Vector2.Distance(transform.position, f.transform.position);
            if (d < md) { md = d; best = f; }
        }
        return best;
    }

    // Ensure lists stay fresh while you remain overlapping
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Sprout") &&
            other.GetComponent<SproutAndLightManager>() != null)
        {
            if (!nearbyFlowers.Contains(other.gameObject))
                nearbyFlowers.Add(other.gameObject);
        }
        else if (other.CompareTag("Garden"))
        {
            var gs = other.GetComponent<GardenSpot>();
            if (gs != null && !nearbyGardens.Contains(gs))
                nearbyGardens.Add(gs);
        }
        else if (teapotReceiver == null)
        {
            var tr = other.GetComponent<TeapotReceiver>()
                  ?? other.attachedRigidbody?.GetComponent<TeapotReceiver>();
            if (tr != null) teapotReceiver = tr;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // mirror Enter for cases where Stay might miss a frame
        OnTriggerStay2D(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Sprout"))
        {
            nearbyFlowers.Remove(other.gameObject);
        }
        else if (other.CompareTag("Garden"))
        {
            var gs = other.GetComponent<GardenSpot>();
            if (gs != null)
            {
                nearbyGardens.Remove(gs);
                if (currentGarden == gs)
                {
                    gs.SetHighlight(false);
                    currentGarden = null;
                }
            }
        }
        else
        {
            var tr = other.GetComponent<TeapotReceiver>()
                  ?? other.attachedRigidbody?.GetComponent<TeapotReceiver>();
            if (tr == teapotReceiver)
                teapotReceiver = null;
        }
    }
}
