// FlowerInteractionManager.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(FlowerHolder))]
public class FlowerInteractionManager : MonoBehaviour
{
    [Header("Hold‑Point (assign same as FlowerHolder)")]
    public Transform holdPoint;

    [Header("Teapot detection tag")]
    public string teapotTag = "Teapot";

    private FlowerHolder flowerHolder;
    private GardenSpot   currentGarden;
    private TeapotReceiver teapotReceiver;

    private readonly List<GameObject> nearbyFlowers = new List<GameObject>();
    private readonly List<GardenSpot>  nearbyGardens  = new List<GardenSpot>();

    void Awake()
    {
        flowerHolder = GetComponent<FlowerHolder>();
        flowerHolder.holdPoint = holdPoint;
    }

    void Update()
    {
        // Teapot priority
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

        // Garden highlight
        HighlightClosestGarden();

        if (Input.GetKeyDown(KeyCode.X))
        {
            if (!flowerHolder.HasFlower)
                TryPickUpFromGardenOrSprout();
            else
                TryPlantToGarden();
        }
    }

    private void TryPickUpFromGardenOrSprout()
    {
        // garden first
        if (currentGarden != null)
        {
            var planted = currentGarden.GetPlantedFlower();
            if (planted != null)
            {
                var pu = planted.GetComponent<FlowerPickup>();
                if (pu != null)
                {
                    // clear old spot
                    pu.CurrentGardenSpot?.ClearPlantedFlower();
                    flowerHolder.PickUpFlower(planted);
                    nearbyFlowers.Remove(planted);
                    return;
                }
            }
        }
        // then loose sprout
        var closest = FindClosestFlower();
        if (closest != null)
        {
            flowerHolder.PickUpFlower(closest);
            nearbyFlowers.Remove(closest);
        }
    }

    private void TryPlantToGarden()
    {
        if (!flowerHolder.HasFlower || currentGarden == null) return;

        // which flower held?
        var heldGO = flowerHolder.GetHeldFlower();
        var heldPU = heldGO.GetComponent<FlowerPickup>();

        // swap out any pre‑placed
        GameObject swapped = currentGarden.GetPlantedFlower();
        if (swapped == heldGO) swapped = null;

        if (swapped == null)
        {
            // scan children for pre‑placed sprout
            for (int i = 0; i < currentGarden.transform.childCount; i++)
            {
                var c = currentGarden.transform.GetChild(i).gameObject;
                if (c == heldGO) continue;
                if (c.GetComponent<FlowerPickup>() != null)
                {
                    swapped = c;
                    break;
                }
            }
        }

        // clear old
        currentGarden.ClearPlantedFlower();

        // plant held
        heldGO.transform.SetParent(currentGarden.plantingPoint, false);
        heldGO.transform.localPosition = Vector3.zero;
        if (heldPU != null)
        {
            heldPU.IsPlanted        = true;
            heldPU.IsHeld           = false;
            heldPU.CurrentGardenSpot = currentGarden;
        }
        var col = heldGO.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
        currentGarden.SetPlantedFlower(heldGO);
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
        var best = nearbyGardens
            .OrderBy(g => Vector2.Distance(transform.position, g.transform.position))
            .FirstOrDefault();
        if (best != currentGarden)
        {
            if (currentGarden != null) currentGarden.SetHighlight(false);
            currentGarden = best;
            if (currentGarden != null) currentGarden.SetHighlight(true);
        }
    }

    private GameObject FindClosestFlower()
    {
        return nearbyFlowers
            .Where(go => go.GetComponent<FlowerPickup>() != null)
            .OrderBy(go => Vector2.Distance(transform.position, go.transform.position))
            .FirstOrDefault();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sprout") && other.GetComponent<FlowerPickup>() != null)
        {
            var go = other.gameObject;
            if (!nearbyFlowers.Contains(go)) nearbyFlowers.Add(go);
        }
        else if (other.CompareTag("Garden"))
        {
            var gs = other.GetComponent<GardenSpot>();
            if (gs != null && !nearbyGardens.Contains(gs)) nearbyGardens.Add(gs);
        }
        else if (other.CompareTag(teapotTag))
        {
            teapotReceiver = other.GetComponent<TeapotReceiver>();
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // ensure continuous detection
        OnTriggerEnter2D(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Sprout"))
            nearbyFlowers.Remove(other.gameObject);
        else if (other.CompareTag("Garden"))
        {
            var gs = other.GetComponent<GardenSpot>();
            if (gs != null)
            {
                nearbyGardens.Remove(gs);
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
