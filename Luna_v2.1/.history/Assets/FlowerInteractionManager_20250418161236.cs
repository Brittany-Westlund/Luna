// FlowerInteractionManager.cs
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(FlowerHolder))]
public class FlowerInteractionManager : MonoBehaviour
{
    [Header("Shared HoldPoint")]
    public Transform holdPoint;

    // Internal refs
    private FlowerHolder   flowerHolder;
    private GardenSpot     currentGarden;
    private TeapotReceiver teapotReceiver;

    void Awake()
    {
        flowerHolder = GetComponent<FlowerHolder>();
        flowerHolder.holdPoint = holdPoint;
    }

    void Update()
    {
        // 1) If we’re inside a teapot, X always goes to pot
        if (teapotReceiver != null)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                // Add if holding, else retrieve
                if (flowerHolder.HasFlower)
                    teapotReceiver.AddFlowerToTeapot(flowerHolder);
                else if (teapotReceiver.HasAnyIngredients())
                    teapotReceiver.RetrieveLastFlower(flowerHolder);
            }
            return;
        }

        // 2) Otherwise garden logic
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
        // Only pick up if we’re actually in a garden trigger
        if (currentGarden == null) return;

        // Find the planted flower for this spot
        GameObject p = currentGarden.GetPlantedFlower();
        if (p == null) return;

        // Clear the spot record
        currentGarden.ClearPlantedFlower();

        // Pick it up
        flowerHolder.PickUpFlower(p);
    }

    private void TryPlantToGarden()
    {
        if (!flowerHolder.HasFlower || currentGarden == null) return;

        GameObject held = flowerHolder.GetHeldFlower();
        GameObject old  = currentGarden.GetPlantedFlower();

        // Only swap if the spot really had a flower distinct from the one we hold
        if (old == held) old = null;

        // Clear the record so we can accept the new one
        currentGarden.ClearPlantedFlower();

        // Plant held
        held.transform.SetParent(currentGarden.transform);
        held.transform.position = currentGarden.GetPlantingPoint().position;
        var mgr = held.GetComponent<SproutAndLightManager>();
        if (mgr != null) { mgr.isPlanted = true; mgr.isHeld = false; }
        var col = held.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
        currentGarden.SetPlantedFlower(held);
        flowerHolder.DropFlower();

        // Swap back if there was actually something planted before
        if (old != null)
            flowerHolder.PickUpFlower(old);
    }

    private void HighlightClosestGarden()
    {
        // Among all overlapping gardens, pick the closest
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0f);
        // (we don’t need radius because the trigger collider on Luna handles it)
        GardenSpot best = null;
        float      md   = float.MaxValue;
        foreach (var col in Physics2D.OverlapCircleAll(transform.position, 0f))
        {
            if (!col.CompareTag("Garden")) continue;
            var gs = col.GetComponent<GardenSpot>();
            if (gs == null) continue;
            float d = Vector2.Distance(transform.position, gs.transform.position);
            if (d < md) { md = d; best = gs; }
        }

        if (best != currentGarden)
        {
            if (currentGarden != null) currentGarden.SetHighlight(false);
            currentGarden = best;
            if (currentGarden != null) currentGarden.SetHighlight(true);
        }
    }

    // Use trigger colliders on Luna for detection
    void OnTriggerEnter2D(Collider2D other)
    {
        // Garden enter
        if (other.CompareTag("Garden"))
        {
            var gs = other.GetComponent<GardenSpot>();
            if (gs != null)
                currentGarden = gs;
            return;
        }

        // Teapot enter
        if (other.CompareTag("Teapot"))
        {
            teapotReceiver = other.GetComponent<TeapotReceiver>();
            return;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            var gs = other.GetComponent<GardenSpot>();
            if (gs == currentGarden)
            {
                gs.SetHighlight(false);
                currentGarden = null;
            }
        }
        else if (other.CompareTag("Teapot"))
        {
            teapotReceiver = null;
        }
    }
}
