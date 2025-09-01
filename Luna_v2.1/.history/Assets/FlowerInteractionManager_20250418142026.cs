// FlowerInteractionManager.cs
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(FlowerHolder))]
public class FlowerInteractionManager : MonoBehaviour
{
    [Header("Hold‑Point (shared with FlowerHolder)")]
    public Transform holdPoint;

    [Header("Tag for your Teapot GameObject")]
    public string teapotTag = "Teapot";

    private FlowerHolder   flowerHolder;
    private GardenSpot     currentGarden;
    private TeapotReceiver teapotReceiver;

    private List<GameObject> nearbyFlowers = new List<GameObject>();
    private List<GardenSpot>  nearbyGardens  = new List<GardenSpot>();

    void Awake()
    {
        flowerHolder = GetComponent<FlowerHolder>();
        flowerHolder.holdPoint = holdPoint;
    }

    void Update()
    {
        // —— Teapot logic first ——
        if (teapotReceiver != null)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                // 1) If holding, add to pot
                if (flowerHolder.HasFlower)
                {
                    teapotReceiver.AddFlowerToTeapot(flowerHolder);
                }
                // 2) Otherwise if pot has flowers, retrieve last
                else if (teapotReceiver.HasAnyIngredients())
                {
                    teapotReceiver.RetrieveLastFlower(flowerHolder);
                }
            }
            return;  // skip garden logic while in the pot
        }

        // —— Garden pickup/plant ——
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
        GameObject closest = FindClosestFlower();
        if (closest == null) return;

        var spr = closest.GetComponent<SproutAndLightManager>();
        if (spr == null) return;

        if (spr.isPlanted && closest.transform.parent != null)
        {
            var oldSpot = closest.transform.parent.GetComponent<GardenSpot>();
            if (oldSpot != null)
                oldSpot.ClearPlantedFlower();
        }

        flowerHolder.PickUpFlower(closest);
        nearbyFlowers.Remove(closest);
    }

    private void TryPlantToGarden()
    {
        if (!flowerHolder.HasFlower || currentGarden == null) return;

        GameObject held    = flowerHolder.GetHeldFlower();
        GameObject swapped = currentGarden.GetPlantedFlower();

        // If the spot truly had that exact flower, we'll swap it out—
        // otherwise swapped stays null.
        if (swapped == held)
            swapped = null;

        // Clear record so we can accept the new one
        currentGarden.ClearPlantedFlower();

        // Parent & position
        held.transform.SetParent(currentGarden.transform);
        held.transform.position = currentGarden.GetPlantingPoint().position;
        var sm = held.GetComponent<SproutAndLightManager>();
        if (sm != null)
        {
            sm.isPlanted = true;
            sm.isHeld    = false;
        }
        var col = held.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        // Record the new flower
        currentGarden.SetPlantedFlower(held);
        flowerHolder.DropFlower();

        // If there was truly something there before, pick it up now
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

        for (int i = 0; i < nearbyGardens.Count; i++)
        {
            var g = nearbyGardens[i];
            if (g == null) continue;
            float d = Vector2.Distance(transform.position, g.transform.position);
            if (d < md)
            {
                md = d;
                best = g;
            }
        }

        if (best != currentGarden)
        {
            if (currentGarden != null)
                currentGarden.SetHighlight(false);
            currentGarden = best;
            if (currentGarden != null)
                currentGarden.SetHighlight(true);
        }
    }

    private GameObject FindClosestFlower()
    {
        GameObject best = null;
        float md = float.MaxValue;

        for (int i = 0; i < nearbyFlowers.Count; i++)
        {
            var f = nearbyFlowers[i];
            if (f == null) continue;
            if (f.GetComponent<SproutAndLightManager>() == null) continue;
            float d = Vector2.Distance(transform.position, f.transform.position);
            if (d < md)
            {
                md   = d;
                best = f;
            }
        }
        return best;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Sprouts
        if (other.CompareTag("Sprout") &&
            other.GetComponent<SproutAndLightManager>() != null)
        {
            var go = other.gameObject;
            if (!nearbyFlowers.Contains(go))
                nearbyFlowers.Add(go);
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
        if (other.CompareTag(teapotTag))
        {
            teapotReceiver = other.GetComponent<TeapotReceiver>();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // mirror OnTriggerEnter so we never lose the reference
        OnTriggerEnter2D(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Sprout"))
            nearbyFlowers.Remove(other.gameObject);
        else if (other.CompareTag("Garden"))
        {
            var gs = other.GetComponent<GardenSpot>();
            if (gs != null && nearbyGardens.Remove(gs))
            {
                if (currentGarden == gs)
                {
                    gs.SetHighlight(false);
                    currentGarden = null;
                }
            }
        }
        else if (other.CompareTag(teapotTag))
        {
            teapotReceiver = null;
        }
    }
}
