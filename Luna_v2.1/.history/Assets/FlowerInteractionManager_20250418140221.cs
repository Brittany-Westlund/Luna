using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(FlowerHolder))]
public class FlowerInteractionManager : MonoBehaviour
{
    [Header("Hold‑Point (shared with FlowerHolder)")]
    public Transform holdPoint;

    [Header("Teapot detection via tag")]
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
        // Teapot has priority
        if (teapotReceiver != null)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                if (!flowerHolder.HasFlower && teapotReceiver.HasAnyIngredients())
                    teapotReceiver.RetrieveLastFlower(flowerHolder);
                else if (flowerHolder.HasFlower)
                    teapotReceiver.AddFlowerToTeapot(flowerHolder);
            }
            return;
        }

        // Garden pickup/plant
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

        var sprout = closest.GetComponent<SproutAndLightManager>();
        if (sprout == null) return;

        if (sprout.isPlanted && closest.transform.parent != null)
        {
            var oldSpot = closest.transform.parent.GetComponent<GardenSpot>();
            if (oldSpot != null) oldSpot.ClearPlantedFlower();
        }

        flowerHolder.PickUpFlower(closest);
        nearbyFlowers.Remove(closest);
    }

    private void TryPlantToGarden()
    {
        if (!flowerHolder.HasFlower || currentGarden == null) return;

        GameObject held    = flowerHolder.GetHeldFlower();
        GameObject swapped = currentGarden.GetPlantedFlower();

        if (swapped == held) swapped = null;

        if (swapped == null)
        {
            int cnt = currentGarden.transform.childCount;
            for (int i = 0; i < cnt; i++)
            {
                var child = currentGarden.transform.GetChild(i).gameObject;
                if (child == held) continue;
                if (child.GetComponent<SproutAndLightManager>() != null)
                {
                    swapped = child;
                    break;
                }
            }
        }

        currentGarden.ClearPlantedFlower();

        held.transform.SetParent(currentGarden.transform);
        held.transform.position = currentGarden.GetPlantingPoint().position;
        var sm = held.GetComponent<SproutAndLightManager>();
        if (sm != null) { sm.isPlanted = true; sm.isHeld = false; }
        var col = held.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
        currentGarden.SetPlantedFlower(held);
        flowerHolder.DropFlower();

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
        float md = float.MaxValue;
        for (int i = 0; i < nearbyFlowers.Count; i++)
        {
            var f = nearbyFlowers[i];
            if (f == null) continue;
            if (f.GetComponent<SproutAndLightManager>() == null) continue;
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
            var go = other.gameObject;
            if (!nearbyFlowers.Contains(go))
                nearbyFlowers.Add(go);
            return;
        }
        if (other.CompareTag("Garden"))
        {
            var gs = other.GetComponent<GardenSpot>();
            if (gs != null && !nearbyGardens.Contains(gs))
                nearbyGardens.Add(gs);
            return;
        }
        if (other.CompareTag(teapotTag))
        {
            teapotReceiver = other.GetComponent<TeapotReceiver>();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
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
