// FlowerInteraction.cs
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(FlowerHolder))]
public class FlowerInteraction : MonoBehaviour
{
    [Header("Hold‑Point (shared with FlowerHolder)")]
    public Transform holdPoint;

    [Header("Automatically assigned at runtime")]
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
        bool holding = flowerHolder.HasFlower;

        // 1) If in teapot range, pressing X adds or retrieves
        if (teapotReceiver != null && teapotReceiver.PlayerIsNearby)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                if (holding)
                {
                    teapotReceiver.AddFlowerToTeapot(flowerHolder);
                }
                else if (teapotReceiver.HasAnyIngredients())
                {
                    teapotReceiver.RetrieveLastFlower(flowerHolder);
                }
            }
            return;
        }

        // 2) Garden pickup/plant logic
        HighlightClosestGarden();

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (!holding)
                TryPickUpFromGarden();
            else
                TryPlantToGarden();
        }
    }

    private void TryPickUpFromGarden()
    {
        GameObject closest = FindClosestFlower();
        if (closest == null) return;

        var sprout = closest.GetComponent<SproutAndLightManager>();
        if (sprout == null) return;

        if (sprout.isPlanted && closest.transform.parent != null)
        {
            var gs = closest.transform.parent.GetComponent<GardenSpot>();
            if (gs != null) gs.ClearPlantedFlower();
        }

        flowerHolder.PickUpFlower(closest);
    }

    private void TryPlantToGarden()
    {
        if (!flowerHolder.HasFlower || currentGarden == null) return;

        GameObject held    = flowerHolder.GetHeldFlower();
        GameObject swapped = currentGarden.GetPlantedFlower();

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

        held.transform.SetParent(currentGarden.transform);
        held.transform.position = currentGarden.GetPlantingPoint().position;
        var s = held.GetComponent<SproutAndLightManager>();
        if (s != null) { s.isPlanted = true; s.isHeld = false; }
        var col = held.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
        currentGarden.SetPlantedFlower(held);
        flowerHolder.DropFlower();

        if (swapped != null)
            flowerHolder.PickUpFlower(swapped);
    }

    private void HighlightClosestGarden()
    {
        GardenSpot best = null;
        float     md   = float.MaxValue;

        foreach (GardenSpot g in nearbyGardens)
        {
            if (g == null) continue;
            float d = Vector2.Distance(transform.position, g.transform.position);
            if (d < md) { md = d; best = g; }
        }

        if (best != currentGarden)
        {
            if (currentGarden != null) currentGarden.SetHighlight(false);
            currentGarden = best;
            if (currentGarden != null) currentGarden.SetHighlight(true);
        }
    }

    private GameObject FindClosestFlower()
    {
        GameObject best = null;
        float      md   = float.MaxValue;

        foreach (GameObject f in nearbyFlowers)
        {
            if (f == null || f.GetComponent<SproutAndLightManager>() == null) continue;
            float d = Vector2.Distance(transform.position, f.transform.position);
            if (d < md) { md = d; best = f; }
        }

        return best;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sprout") &&
            other.GetComponent<SproutAndLightManager>() != null)
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

        if (teapotReceiver == null)
        {
            TeapotReceiver tr = other.GetComponent<TeapotReceiver>();
            if (tr == null && other.attachedRigidbody != null)
                tr = other.attachedRigidbody.GetComponent<TeapotReceiver>();
            if (tr != null)
                teapotReceiver = tr;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Sprout"))
            nearbyFlowers.Remove(other.gameObject);
        else if (other.CompareTag("Garden"))
        {
            var gs = other.GetComponent<GardenSpot>();
            if (gs != null && nearbyGardens.Contains(gs))
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
            TeapotReceiver tr = other.GetComponent<TeapotReceiver>();
            if (tr == null && other.attachedRigidbody != null)
                tr = other.attachedRigidbody.GetComponent<TeapotReceiver>();
            if (tr == teapotReceiver)
                teapotReceiver = null;
        }
    }
}
