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

        // —— Garden highlight & plant/pickup ——
        UpdateGardenHighlighting();

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (!flowerHolder.HasFlower)
                TryPickUpFromGarden();
            else
                TryPlantToGarden();
        }
    }

    private void UpdateGardenHighlighting()
    {
        GardenSpot best = null;
        float      md   = float.MaxValue;

        foreach (var gs in nearbyGardens)
        {
            if (gs == null) continue;
            float d = Vector2.Distance(transform.position, gs.transform.position);
            if (d < md)
            {
                md   = d;
                best = gs;
            }
        }

        // highlight only the best
        foreach (var gs in nearbyGardens)
            if (gs != null)
                gs.SetHighlight(gs == best);

        currentGarden = best;
    }

    private void TryPickUpFromGarden()
    {
        if (currentGarden == null) return;
        GameObject flower = currentGarden.GetPlantedFlower();
        if (flower == null) return;

        currentGarden.ClearPlantedFlower();
        flowerHolder.PickUpFlower(flower);
    }

    private void TryPlantToGarden()
    {
        if (currentGarden == null || !flowerHolder.HasFlower) return;

        GameObject held = flowerHolder.GetHeldFlower();
        GameObject old  = currentGarden.GetPlantedFlower();

        if (old == held) old = null;
        currentGarden.ClearPlantedFlower();

        // ✅ Parent with world transform preserved
        held.transform.SetParent(currentGarden.transform, true);

        // ✅ Snap to world planting point
        held.transform.position = currentGarden.GetPlantingPoint().position;

        // ✅ Fully reset scale — avoids skew from inherited transforms
        held.transform.localScale = Vector3.one;
        held.transform.localRotation = Quaternion.identity;

        // ✅ Update planting state
        var sm = held.GetComponent<SproutAndLightManager>();
        if (sm != null)
        {
            sm.isPlanted = true;
            sm.isHeld = false;
        }

        // ✅ Re-enable collider
        var col = held.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        currentGarden.SetPlantedFlower(held);
        flowerHolder.DropFlower();

        if (old != null)
            flowerHolder.PickUpFlower(old);
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
