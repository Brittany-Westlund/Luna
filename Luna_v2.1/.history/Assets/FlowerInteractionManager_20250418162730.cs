// FlowerInteractionManager.cs
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(FlowerHolder))]
public class FlowerInteractionManager : MonoBehaviour
{
    [Header("Hold‑Point (shared with FlowerHolder)")]
    public Transform holdPoint;

    private FlowerHolder             flowerHolder;
    private TeapotReceiver           teapotReceiver;
    private readonly List<GardenSpot> nearbyGardens  = new List<GardenSpot>();

    void Awake()
    {
        flowerHolder = GetComponent<FlowerHolder>();
        flowerHolder.holdPoint = holdPoint;
    }

    void Update()
    {
        // 1) Teapot has top priority
        if (teapotReceiver != null)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                if (flowerHolder.HasFlower)
                    teapotReceiver.AddFlowerToTeapot(flowerHolder);
                else if (teapotReceiver.HasAnyIngredients())
                    teapotReceiver.RetrieveLastFlower(flowerHolder);
            }
            return;
        }

        // 2) Garden highlight & pickup/plant
        UpdateGardenHighlights();

        if (Input.GetKeyDown(KeyCode.X))
        {
            GameObject held = flowerHolder.GetHeldFlower();
            if (held == null)
                TryPickUpFromGarden();
            else
                TryPlantToGarden();
        }
    }

    private void UpdateGardenHighlights()
    {
        // Find the closest garden in nearbyGardens
        GardenSpot best = null;
        float bestDist = float.MaxValue;
        foreach (var gs in nearbyGardens)
        {
            if (gs == null) continue;
            float d = Vector2.Distance(transform.position, gs.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = gs;
            }
        }

        // Highlight only the best, un‑highlight the rest
        foreach (var gs in nearbyGardens)
        {
            if (gs != null)
                gs.SetHighlight(gs == best);
        }
    }

    private void TryPickUpFromGarden()
    {
        // pick from the currently highlighted garden (closest one)
        GardenSpot gs = null;
        foreach (var g in nearbyGardens)
        {
            if (g == null) continue;
            if (g == nearbyGardens.Find(x => x.GetHighlight()))  // using highlight state
            {
                gs = g;
                break;
            }
        }
        // simpler: just re‑compute best
        float bestDist = float.MaxValue;
        foreach (var g in nearbyGardens)
        {
            if (g == null) continue;
            float d = Vector2.Distance(transform.position, g.transform.position);
            if (d < bestDist) { bestDist = d; gs = g; }
        }

        if (gs == null) return;

        GameObject planted = gs.GetPlantedFlower();
        if (planted == null) return;

        gs.ClearPlantedFlower();
        flowerHolder.PickUpFlower(planted);
    }

    private void TryPlantToGarden()
    {
        // plant into the closest garden
        GardenSpot gs = null;
        float bestDist = float.MaxValue;
        foreach (var g in nearbyGardens)
        {
            if (g == null) continue;
            float d = Vector2.Distance(transform.position, g.transform.position);
            if (d < bestDist) { bestDist = d; gs = g; }
        }
        if (gs == null) return;

        GameObject held = flowerHolder.GetHeldFlower();
        GameObject old  = gs.GetPlantedFlower();

        if (old == held) old = null;

        gs.ClearPlantedFlower();

        held.transform.SetParent(gs.transform);
        held.transform.position = gs.GetPlantingPoint().position;
        var sm = held.GetComponent<SproutAndLightManager>();
        if (sm != null) { sm.isPlanted = true; sm.isHeld = false; }
        var col = held.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
        gs.SetPlantedFlower(held);
        flowerHolder.DropFlower();

        if (old != null)
            flowerHolder.PickUpFlower(old);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            var gs = other.GetComponent<GardenSpot>();
            if (gs != null && !nearbyGardens.Contains(gs))
                nearbyGardens.Add(gs);
        }
        else if (other.CompareTag("Teapot"))
        {
            teapotReceiver = other.GetComponent<TeapotReceiver>();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            var gs = other.GetComponent<GardenSpot>();
            if (gs != null && nearbyGardens.Remove(gs))
                gs.SetHighlight(false);
        }
        else if (other.CompareTag("Teapot"))
        {
            teapotReceiver = null;
        }
    }
}
