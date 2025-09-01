// FlowerInteractionManager.cs
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(FlowerHolder))]
public class FlowerInteractionManager : MonoBehaviour
{
    [Header("Hold‑Point (shared with FlowerHolder)")]
    public Transform holdPoint;

    private FlowerHolder   flowerHolder;
    private GardenSpot     currentGarden;
    private TeapotReceiver teapotReceiver;

    // Gardens you’re standing over
    private readonly List<GardenSpot> nearbyGardens = new List<GardenSpot>();

    void Awake()
    {
        flowerHolder = GetComponent<FlowerHolder>();
        flowerHolder.holdPoint = holdPoint;
    }

    void Update()
    {
        // —— Teapot logic (must stay in its trigger) ——
        if (teapotReceiver != null)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                if (flowerHolder.HasFlower)
                    teapotReceiver.AddFlowerToTeapot(flowerHolder);
                else if (teapotReceiver.HasAnyIngredients())
                    teapotReceiver.RetrieveLastFlower(flowerHolder);
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                Debug.Log($"🌸 X was pressed. Resting = {FindObjectOfType<LunaRest>()?.isResting ?? false}");
            }

            return;

        }
    }

    // —— Garden highlight & plant/pickup ——
    private void UpdateGardenHighlighting()
    {
        GardenSpot best = null;
        float md = float.MaxValue;

        foreach (var gs in nearbyGardens)
        {
            if (gs == null) continue;

            // 🔄 Use distance to plantingPoint instead of gs.transform.position
            float d = Vector2.Distance(transform.position, gs.GetPlantingPoint().position);
            if (d < md)
            {
                md = d;
                best = gs;
            }
        }

        // Highlight only the best one
        foreach (var gs in nearbyGardens)
        {
            if (gs != null)
                gs.SetHighlight(gs == best);
        }

        currentGarden = best;
    }

    private void TryPickUpFromGarden()
    {
        if (currentGarden == null) return;

        GameObject flower = currentGarden.GetPlantedFlower();
        if (flower == null) return;

        // 💥 Detach flower logically before pickup
        currentGarden.ClearPlantedFlower();

        // ✅ Pick it up
        flowerHolder.PickUpFlower(flower);
    }

    private void TryPlantToGarden()
    {
        // Ensure there's a current garden and a flower to plant
        if (currentGarden == null || !flowerHolder.HasFlower) return;

        // Get the currently held flower
        GameObject heldFlower = flowerHolder.GetHeldFlower();
        if (heldFlower == null) return;

        // Retrieve any existing flower in the garden
        GameObject existingFlower = currentGarden.GetPlantedFlower();

        // Avoid replanting the same flower
        if (existingFlower == heldFlower) existingFlower = null;

        // Clear the garden's current flower
        currentGarden.ClearPlantedFlower();

        // Parent the held flower to the garden while preserving world position
        heldFlower.transform.SetParent(currentGarden.transform, true);

        // Snap the held flower to the garden's planting point
        heldFlower.transform.position = currentGarden.GetPlantingPoint().position;

        // Update the flower's state to indicate it's planted
        var sproutManager = heldFlower.GetComponent<SproutAndLightManager>();
        if (sproutManager != null)
        {
            sproutManager.isPlanted = true;
            sproutManager.isHeld = false;
        }

        // Re-enable the flower's collider for interactions
        var collider = heldFlower.GetComponent<Collider2D>();
        if (collider != null) collider.enabled = true;

        // Set the held flower as the garden's planted flower
        currentGarden.SetPlantedFlower(heldFlower);

        // Drop the flower from the holder
        flowerHolder.DropFlower();

        // If there was an existing flower, pick it up
        if (existingFlower != null)
        {
            flowerHolder.PickUpFlower(existingFlower);
        }
    } 

    public void PickUpFlower(GameObject flower)
    {
        if (flowerHolder.HasFlower) return;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = true;

        flowerHolder.PlayPickupSFX();

        flower.transform.SetParent(holdPoint);
        flower.transform.localPosition = Vector3.zero;
        flower.transform.localRotation = Quaternion.identity;

        if (flower.TryGetComponent<Collider2D>(out var col))
            col.enabled = false;

        // No need to reassign — handled internally
        // flowerHolder.heldFlower = flower; ❌ NO!
    }


    /* private void TryPlantToGarden()
    {
        if (currentGarden == null || !flowerHolder.HasFlower) return;

        GameObject held = flowerHolder.GetHeldFlower();
        GameObject old  = currentGarden.GetPlantedFlower();

        if (old == held) old = null;
        currentGarden.ClearPlantedFlower();

        // place held
        held.transform.SetParent(currentGarden.transform, true); // keepWorldPosition = true
        held.transform.position = currentGarden.GetPlantingPoint().position;
        var sm = held.GetComponent<SproutAndLightManager>();
        if (sm != null) { sm.isPlanted = true; sm.isHeld = false; }
        var col = held.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
        currentGarden.SetPlantedFlower(held);
        flowerHolder.DropFlower();

        // swap back
        if (old != null)
            flowerHolder.PickUpFlower(old);
    } */

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

    void OnTriggerStay2D(Collider2D other)
    {
        // keep the teapotReceiver alive while inside
        if (other.CompareTag("Teapot"))
            teapotReceiver = other.GetComponent<TeapotReceiver>();

        if (other.CompareTag("Garden"))
        {
            var gs = other.GetComponent<GardenSpot>();
            if (gs != null && !nearbyGardens.Contains(gs))
                nearbyGardens.Add(gs);
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
