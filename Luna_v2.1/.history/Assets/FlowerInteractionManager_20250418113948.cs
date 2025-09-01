using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(FlowerHolder))]
public class FlowerInteraction : MonoBehaviour
{
    [Header("Hold‑Point (shared with FlowerHolder)")]
    public Transform holdPoint;

    [Header("Auto‑detect TeapotReceiver")]
    public TeapotReceiver teapotReceiver;

    private FlowerHolder flowerHolder;
    private GardenSpot currentGarden;
    private readonly List<GameObject> nearbyFlowers = new List<GameObject>();
    private readonly List<GardenSpot> nearbyGardens  = new List<GardenSpot>();

    void Awake()
    {
        flowerHolder = GetComponent<FlowerHolder>();
        flowerHolder.holdPoint = holdPoint;
    }

    void Update()
    {
        // If we're in range of a teapot, let TeapotReceiver handle X entirely
        if (teapotReceiver != null && teapotReceiver.PlayerIsNearby)
            return;

        // Otherwise, do garden pickup/plant
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

        if (sprout.isPlanted && closest.transform.parent != null)
        {
            var gs = closest.transform.parent.GetComponent<GardenSpot>();
            gs?.ClearPlantedFlower();
        }

        flowerHolder.PickUpFlower(closest);
    }

    private void TryPlantToGarden()
    {
        if (!flowerHolder.HasFlower || currentGarden == null) return;

        var held    = flowerHolder.GetHeldFlower();
        GameObject swapped = currentGarden.GetPlantedFlower();
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

        // plant held
        held.transform.SetParent(currentGarden.transform);
        held.transform.position = currentGarden.GetPlantingPoint().position;
        var s = held.GetComponent<SproutAndLightManager>();
        if (s != null) { s.isPlanted = true; s.isHeld = false; }
        held.GetComponent<Collider2D>()?.enabled = true;
        currentGarden.SetPlantedFlower(held);
        flowerHolder.DropFlower();

        // swap back
        if (swapped != null)
            flowerHolder.PickUpFlower(swapped);
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
            if (f == null || f.GetComponent<SproutAndLightManager>() == null) continue;
            float d = Vector2.Distance(transform.position, f.transform.position);
            if (d < md) { md = d; best = f; }
        }
        return best;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sprout") && other.GetComponent<SproutAndLightManager>() != null)
        {
            if (!nearbyFlowers.Contains(other.gameObject))
                nearbyFlowers.Add(other.gameObject);
            return;
        }

        if (other.CompareTag("Garden"))
        {
            var gs = other.GetComponent<GardenSpot>();
            if (gs != null && !nearbyGardens.Contains(gs))
                nearbyGardens.Add(gs);
            return;
        }

        // Auto‑detect teapot receiver
        if (teapotReceiver == null)
        {
            var tr = other.GetComponent<TeapotReceiver>()
                  ?? other.attachedRigidbody?.GetComponent<TeapotReceiver>();
            if (tr != null) teapotReceiver = tr;
        }
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
            if (tr == teapotReceiver)
                teapotReceiver = null;
        }
    }
}
