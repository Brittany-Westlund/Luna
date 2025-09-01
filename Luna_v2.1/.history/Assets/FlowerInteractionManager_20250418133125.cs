// FlowerInteractionManager.cs
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(FlowerHolder))]
public class FlowerInteractionManager : MonoBehaviour
{
    [Header("Hold‑Point (shared with FlowerHolder)")]
    public Transform holdPoint;

    [Header("Teapot detection via tag")]
    public string teapotTag = "Teapot";

    private FlowerHolder flowerHolder;
    private GardenSpot   currentGarden;
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
        // --- Teapot add/retrieve on a single X ---
        if (teapotReceiver != null)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                if (flowerHolder.HasFlower)
                {
                    teapotReceiver.AddFlowerToTeapot(flowerHolder);
                }
                else if (teapotReceiver.HasAnyIngredients())
                {
                    teapotReceiver.RetrieveLastFlower(flowerHolder);
                }
            }
            return;  // skip garden logic while in the pot
        }

        // --- Garden pickup / plant ---
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

        SproutAndLightManager sprout = closest.GetComponent<SproutAndLightManager>();
        if (sprout == null) return;

        if (sprout.isPlanted && closest.transform.parent != null)
        {
            GardenSpot oldSpot = closest.transform.parent.GetComponent<GardenSpot>();
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

        // only swap if GardenSpot really had that flower
        if (swapped == held) swapped = null;

        // detect any pre‑placed child under this spot
        if (swapped == null)
        {
            int cnt = currentGarden.transform.childCount;
            for (int i = 0; i < cnt; i++)
            {
                GameObject child = currentGarden.transform.GetChild(i).gameObject;
                if (child == held) continue;
                if (child.GetComponent<SproutAndLightManager>() != null)
                {
                    swapped = child;
                    break;
                }
            }
        }

        currentGarden.ClearPlantedFlower();

        // plant held
        held.transform.SetParent(currentGarden.transform);
        held.transform.position = currentGarden.GetPlantingPoint().position;
        SproutAndLightManager sm = held.GetComponent<SproutAndLightManager>();
        if (sm != null) { sm.isPlanted = true; sm.isHeld = false; }
        Collider2D col = held.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
        currentGarden.SetPlantedFlower(held);
        flowerHolder.DropFlower();

        // swap back if needed
        if (swapped != null)
        {
            flowerHolder.PickUpFlower(swapped);
            if (!nearbyFlowers.Contains(swapped))
                nearbyFlowers.Add(swapped);
        }
    }

    private void HighlightClosestGarden()
    {
        GardenSpot best = null; float md = float.MaxValue;
        for (int i = 0; i < nearbyGardens.Count; i++)
        {
            GardenSpot g = nearbyGardens[i];
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
        GameObject best = null; float md = float.MaxValue;
        for (int i = 0; i < nearbyFlowers.Count; i++)
        {
            GameObject f = nearbyFlowers[i];
            if (f == null) continue;
            if (f.GetComponent<SproutAndLightManager>() == null) continue;
            float d = Vector2.Distance(transform.position, f.transform.position);
            if (d < md) { md = d; best = f; }
        }
        return best;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // loose flowers
        if (other.CompareTag("Sprout") &&
            other.GetComponent<SproutAndLightManager>() != null)
        {
            GameObject go = other.gameObject;
            if (!nearbyFlowers.Contains(go))
                nearbyFlowers.Add(go);
            return;
        }
        // gardens
        if (other.CompareTag("Garden"))
        {
            GardenSpot gs = other.GetComponent<GardenSpot>();
            if (gs != null && !nearbyGardens.Contains(gs))
                nearbyGardens.Add(gs);
            return;
        }
        // teapot
        if (other.CompareTag(teapotTag))
        {
            teapotReceiver = other.GetComponent<TeapotReceiver>();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // mirror Enter so nothing ever drops out
        OnTriggerEnter2D(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Sprout"))
            nearbyFlowers.Remove(other.gameObject);
        else if (other.CompareTag("Garden"))
        {
            GardenSpot gs = other.GetComponent<GardenSpot>();
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
