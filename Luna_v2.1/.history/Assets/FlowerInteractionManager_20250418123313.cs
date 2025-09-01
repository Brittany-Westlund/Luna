using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(FlowerHolder))]
public class FlowerInteractionManager : MonoBehaviour
{
    [Header("Hold‑Point (shared with FlowerHolder)")]
    public Transform holdPoint;

    [Header("Auto‑detect TeapotReceiver at runtime")]
    public TeapotReceiver teapotReceiver;

    private FlowerHolder flowerHolder;
    private GardenSpot   currentGarden;
    private readonly List<GameObject> nearbyFlowers = new List<GameObject>();
    private readonly List<GardenSpot>  nearbyGardens  = new List<GardenSpot>();

    void Awake()
    {
        flowerHolder = GetComponent<FlowerHolder>();
        flowerHolder.holdPoint = holdPoint;
    }

    void Update()
    {
        // 1) If teapot in range, handle X for add/retrieve
        if (teapotReceiver != null && teapotReceiver.PlayerIsNearby)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                if (flowerHolder.HasFlower)
                    teapotReceiver.AddFlowerToTeapot(flowerHolder);
                else if (teapotReceiver.HasAnyIngredients())
                    teapotReceiver.RetrieveLastFlower(flowerHolder);
            }
            return; // skip garden logic while at pot
        }

        // 2) Otherwise, garden pickup/plant
        HighlightClosestGarden();
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (!flowerHolder.HasFlower)
                TryPickUpFromGarden();
            else
                TryPlantToGarden();
        }
    }

    private void TryPickUpFromGarden()
    {
        var closest = FindClosestFlower();
        if (closest == null) return;

        var sprout = closest.GetComponent<SproutAndLightManager>();
        if (sprout == null) return;

        // Clear its old garden if planted
        if (sprout.isPlanted && closest.transform.parent != null)
        {
            var oldSpot = closest.transform.parent.GetComponent<GardenSpot>();
            oldSpot?.ClearPlantedFlower();
        }

        flowerHolder.PickUpFlower(closest);
        nearbyFlowers.Remove(closest);
    }

    private void TryPlantToGarden()
    {
        if (!flowerHolder.HasFlower || currentGarden == null) return;

        var held    = flowerHolder.GetHeldFlower();
        GameObject swapped = currentGarden.GetPlantedFlower();

        // Avoid swapping the same object
        if (swapped == held) swapped = null;

        // If nothing tracked, scan children
        if (swapped == null)
        {
            foreach (Transform c in currentGarden.transform)
                if (c.gameObject != held && c.GetComponent<SproutAndLightManager>() != null)
                {
                    swapped = c.gameObject;
                    break;
                }
        }

        currentGarden.ClearPlantedFlower();

        // Plant
        held.transform.SetParent(currentGarden.transform);
        held.transform.position = currentGarden.GetPlantingPoint().position;
        var sm = held.GetComponent<SproutAndLightManager>();
        if (sm != null) { sm.isPlanted = true; sm.isHeld = false; }
        var col = held.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
        currentGarden.SetPlantedFlower(held);
        flowerHolder.DropFlower();

        // Immediately pick up swapped
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
        float     md   = float.MaxValue;
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
        float      md   = float.MaxValue;
        foreach (var f in nearbyFlowers)
        {
            if (f == null) continue;
            if (f.GetComponent<SproutAndLightManager>() == null) continue;
            float d = Vector2.Distance(transform.position, f.transform.position);
            if (d < md) { md = d; best = f; }
        }
        return best;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Sprouts
        if (other.CompareTag("Sprout") &&
            other.GetComponent<SproutAndLightManager>() != null)
        {
            if (!nearbyFlowers.Contains(other.gameObject))
                nearbyFlowers.Add(other.gameObject);
            return;
        }

        // Gardens
        if (other.CompareTag("Garden"))
        {
            var gs = other.GetComponent<GardenSpot>();
            if (gs != null && !nearbyGardens.Contains(gs))
                nearbyGardens.Add(gs);
            return;
        }

        // Teapot
        if (teapotReceiver == null)
        {
            var tr = other.GetComponent<TeapotReceiver>()
                  ?? other.attachedRigidbody?.GetComponent<TeapotReceiver>();
            if (tr != null) teapotReceiver = tr;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // Mirror Enter logic so lists never go stale
        OnTriggerEnter2D(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Sprout"))
            nearbyFlowers.Remove(other.gameObject);
        else if (other.CompareTag("Garden"))
        {
            var gs = other.GetComponent<GardenSpot>();
            if (gs != null && nearbyGardens.Remove(gs) && currentGarden == gs)
            {
                gs.SetHighlight(false);
                currentGarden = null;
            }
        }
        else
        {
            var tr = other.GetComponent<TeapotReceiver>()
                  ?? other.attachedRigidbody?.GetComponent<TeapotReceiver>();
            if (tr == teapotReceiver) teapotReceiver = null;
        }
    }
}
