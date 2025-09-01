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
    private readonly List<GardenSpot> nearbyGardens = new List<GardenSpot>();
    private GardenSpot               currentGarden;

    void Awake()
    {
        flowerHolder = GetComponent<FlowerHolder>();
        flowerHolder.holdPoint = holdPoint;
    }

    void Update()
    {
        // 1) Teapot logic
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

        // 2) Garden logic
        UpdateGardenHighlights();

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (!flowerHolder.HasFlower)
                TryPickUpFromGarden();
            else
                TryPlantToGarden();
        }
    }

    private void UpdateGardenHighlights()
    {
        // Find closest garden in range
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

        // Highlight only the best, un‑highlight the previous
        if (best != currentGarden)
        {
            if (currentGarden != null)
                currentGarden.SetHighlight(false);
            currentGarden = best;
            if (currentGarden != null)
                currentGarden.SetHighlight(true);
        }
    }

    private void TryPickUpFromGarden()
    {
        if (currentGarden == null)
            return;

        GameObject planted = currentGarden.GetPlantedFlower();
        if (planted == null)
            return;

        currentGarden.ClearPlantedFlower();
        flowerHolder.PickUpFlower(planted);
    }

    private void TryPlantToGarden()
    {
        if (currentGarden == null || !flowerHolder.HasFlower)
            return;

        GameObject held = flowerHolder.GetHeldFlower();
        GameObject old  = currentGarden.GetPlantedFlower();

        // Only swap if the garden truly held a *different* flower
        if (old == held) old = null;

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

        // Swap back
        if (old != null)
            flowerHolder.PickUpFlower(old);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            GardenSpot gs = other.GetComponent<GardenSpot>();
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
            GardenSpot gs = other.GetComponent<GardenSpot>();
            if (gs != null && nearbyGardens.Remove(gs))
            {
                gs.SetHighlight(false);
                if (currentGarden == gs)
                    currentGarden = null;
            }
        }
        else if (other.CompareTag("Teapot"))
        {
            teapotReceiver = null;
        }
    }
}
