// FlowerInteraction.cs
using UnityEngine;
using System.Collections.Generic;

public class FlowerInteraction : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Transform holdPoint;

    [Header("Auto‑detect TeapotReceiver")]
    public TeapotReceiver teapotReceiver;

    private GameObject heldFlower;
    private GardenSpot currentGarden;
    private readonly List<GameObject> nearbyFlowers = new();
    private readonly List<GardenSpot> nearbyGardens = new();

    void Update()
    {
        // if holding + teapot in range, let the teapot get X
        if (heldFlower != null && teapotReceiver != null && teapotReceiver.PlayerIsNearby)
            return;

        HighlightClosestGarden();

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (heldFlower == null)
                TryPickUpFlower();
            else
                TryPlantFlower();
        }
    }

    void TryPickUpFlower()
    {
        var closest = FindClosestFlower();
        if (closest == null) return;

        if (closest.TryGetComponent<SproutAndLightManager>(out var sprout))
        {
            if (sprout.isPlanted && closest.transform.parent != null)
                closest.transform.parent.GetComponent<GardenSpot>()?.ClearPlantedFlower();

            sprout.isHeld = true;
            sprout.isPlanted = false;
            heldFlower = closest;

            closest.transform.SetParent(holdPoint);
            closest.transform.localPosition = Vector3.zero;
            if (closest.TryGetComponent<Collider2D>(out var col)) col.enabled = false;
        }
    }

    void TryPlantFlower()
    {
        if (heldFlower == null || currentGarden == null) return;

        var garden = currentGarden;
        // find swapped
        GameObject swapped = garden.GetPlantedFlower();
        if (swapped == null)
        {
            foreach (Transform c in garden.transform)
                if (c.gameObject != heldFlower && c.TryGetComponent<SproutAndLightManager>(out _))
                {
                    swapped = c.gameObject;
                    break;
                }
        }

        garden.ClearPlantedFlower();

        // plant
        heldFlower.transform.SetParent(garden.transform);
        heldFlower.transform.position = garden.GetPlantingPoint().position;
        if (heldFlower.TryGetComponent<SproutAndLightManager>(out var os))
        { os.isPlanted = true; os.isHeld = false; }
        if (heldFlower.TryGetComponent<Collider2D>(out var oc)) oc.enabled = true;
        garden.SetPlantedFlower(heldFlower);

        // swap back
        if (swapped != null)
        {
            if (swapped.TryGetComponent<SproutAndLightManager>(out var ss))
            { ss.isHeld = true; ss.isPlanted = false; }

            swapped.transform.SetParent(holdPoint);
            swapped.transform.localPosition = Vector3.zero;
            if (swapped.TryGetComponent<Collider2D>(out var sc)) sc.enabled = false;

            heldFlower = swapped;
        }
        else heldFlower = null;
    }

    void HighlightClosestGarden()
    {
        GardenSpot best = null;
        float md = float.MaxValue;
        foreach (var g in nearbyGardens)
            if (g != null)
            {
                float d = Vector2.Distance(transform.position, g.transform.position);
                if (d < md) { md = d; best = g; }
            }

        if (best != currentGarden)
        {
            currentGarden?.SetHighlight(false);
            currentGarden = best;
            currentGarden?.SetHighlight(true);
        }
    }

    GameObject FindClosestFlower()
    {
        GameObject best = null;
        float md = float.MaxValue;
        foreach (var f in nearbyFlowers)
            if (f != null && f != heldFlower && f.TryGetComponent<SproutAndLightManager>(out _))
            {
                float d = Vector2.Distance(transform.position, f.transform.position);
                if (d < md) { md = d; best = f; }
            }
        return best;
    }

    void OnTriggerEnter2D(Collider2D o)
    {
        if (o.CompareTag("Sprout") && o.TryGetComponent<SproutAndLightManager>(out _))
        {
            if (!nearbyFlowers.Contains(o.gameObject))
                nearbyFlowers.Add(o.gameObject);
        }
        else if (o.CompareTag("Garden"))
        {
            var g = o.GetComponent<GardenSpot>();
            if (g != null && !nearbyGardens.Contains(g))
                nearbyGardens.Add(g);
        }
        else if (teapotReceiver == null)
        {
            // auto‑hook the receiver
            var tr = o.GetComponent<TeapotReceiver>() 
                  ?? o.attachedRigidbody?.GetComponent<TeapotReceiver>();
            if (tr != null)
                teapotReceiver = tr;
        }
    }

    void OnTriggerExit2D(Collider2D o)
    {
        if (o.CompareTag("Sprout"))
            nearbyFlowers.Remove(o.gameObject);
        else if (o.CompareTag("Garden"))
        {
            var g = o.GetComponent<GardenSpot>();
            if (g != null && nearbyGardens.Remove(g) && currentGarden == g)
            {
                g.SetHighlight(false);
                currentGarden = null;
            }
        }
        else
        {
            var tr = o.GetComponent<TeapotReceiver>()
                  ?? o.attachedRigidbody?.GetComponent<TeapotReceiver>();
            if (tr == teapotReceiver)
                teapotReceiver = null;
        }
    }
}
