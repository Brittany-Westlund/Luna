// FlowerInteractionManager.cs
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(FlowerHolder))]
public class FlowerInteractionManager : MonoBehaviour
{
    [Header("Hold‑Point (shared with FlowerHolder)")]
    public Transform holdPoint;

    [Header("Auto‑detect TeapotReceiver")]
    public TeapotReceiver teapotReceiver;

    private FlowerHolder flowerHolder;
    private GardenSpot currentGarden;
    private List<GameObject> nearbyFlowers = new List<GameObject>();
    private List<GardenSpot>  nearbyGardens  = new List<GardenSpot>();

    void Awake()
    {
        flowerHolder = GetComponent<FlowerHolder>();
        flowerHolder.holdPoint = holdPoint;
    }

    void Update()
    {
        // If we're in teapot range, let TeapotReceiver handle X entirely
        if (teapotReceiver != null && teapotReceiver.PlayerIsNearby)
            return;

        // Otherwise, garden pickup/plant
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
    }

    private void TryPlantToGarden()
    {
        if (!flowerHolder.HasFlower || currentGarden == null) return;

        GameObject heldFlower = flowerHolder.GetHeldFlower();
        GameObject swapped     = currentGarden.GetPlantedFlower();

        if (swapped == null)
        {
            // detect any child that’s planted
            foreach (Transform child in currentGarden.transform)
            {
                if (child.gameObject != heldFlower &&
                    child.GetComponent<SproutAndLightManager>() != null)
                {
                    swapped = child.gameObject;
                    break;
                }
            }
        }

        currentGarden.ClearPlantedFlower();

        // plant our held flower
        heldFlower.transform.SetParent(currentGarden.transform);
        heldFlower.transform.position = currentGarden.GetPlantingPoint().position;
        var sm = heldFlower.GetComponent<SproutAndLightManager>();
        if (sm != null) { sm.isPlanted = true; sm.isHeld = false; }
        var col = heldFlower.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
        currentGarden.SetPlantedFlower(heldFlower);
        flowerHolder.DropFlower();

        // pick up the swapped one (if any)
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

        // auto‑detect the teapot
        if (teapotReceiver == null)
        {
            var tr = other.GetComponent<TeapotReceiver>();
            if (tr == null && other.attachedRigidbody != null)
                tr = other.attachedRigidbody.GetComponent<TeapotReceiver>();
            if (tr != null) teapotReceiver = tr;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Sprout"))
        {
            nearbyFlowers.Remove(other.gameObject);
        }
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
            var tr = other.GetComponent<TeapotReceiver>();
            if (tr == null && other.attachedRigidbody != null)
                tr = other.attachedRigidbody.GetComponent<TeapotReceiver>();
            if (tr == teapotReceiver) teapotReceiver = null;
        }
    }

    void OnTriggerStay2D(Collider2D other)
{
    // Sprout still in range?
    if (other.CompareTag("Sprout") &&
        other.GetComponent<SproutAndLightManager>() != null)
    {
        if (!nearbyFlowers.Contains(other.gameObject))
            nearbyFlowers.Add(other.gameObject);
    }

    // Garden still in range?
    if (other.CompareTag("Garden"))
    {
        var gs = other.GetComponent<GardenSpot>();
        if (gs != null && !nearbyGardens.Contains(gs))
            nearbyGardens.Add(gs);
    }

    // Re‑hook the teapot if we walk into it (just in case)
    if (teapotReceiver == null)
    {
        var tr = other.GetComponent<TeapotReceiver>();
        if (tr == null && other.attachedRigidbody != null)
            tr = other.attachedRigidbody.GetComponent<TeapotReceiver>();
        if (tr != null)
            teapotReceiver = tr;
    }
}



}
