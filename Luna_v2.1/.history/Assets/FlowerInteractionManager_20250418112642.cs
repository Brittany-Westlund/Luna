using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(FlowerHolder))]
public class FlowerInteraction : MonoBehaviour
{
    [Header("Hold‑Point (same as FlowerHolder)")]
    public Transform holdPoint;

    [Header("Auto‑detect TeapotReceiver")]
    public TeapotReceiver teapotReceiver;

    private FlowerHolder flowerHolder;
    private GardenSpot currentGarden;
    private readonly List<GameObject> nearbyFlowers = new List<GameObject>();
    private readonly List<GardenSpot> nearbyGardens = new List<GardenSpot>();

    void Awake()
    {
        flowerHolder = GetComponent<FlowerHolder>();
        flowerHolder.holdPoint = holdPoint;
    }

    void Update()
    {
        bool holding = flowerHolder.HasFlower;

        // 1) If holding & teapot in range, pressing X adds/removes from pot
        if (holding && teapotReceiver != null && teapotReceiver.PlayerIsNearby)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                if (flowerHolder.HasFlower)
                {
                    teapotReceiver.AddFlowerToTeapot(flowerHolder);
                }
                else if (teapotReceiver.HasAnyIngredients())
                {
                    teapotReceiver.RetrieveLastFlower();
                }
            }
            return;
        }

        // 2) Garden pickup/plant
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

        SproutAndLightManager sprout = closest.GetComponent<SproutAndLightManager>();
        if (sprout == null) return;

        // clear old garden slot
        if (sprout.isPlanted && closest.transform.parent != null)
        {
            GardenSpot oldGarden = closest.transform.parent.GetComponent<GardenSpot>();
            if (oldGarden != null)
                oldGarden.ClearPlantedFlower();
        }

        flowerHolder.PickUpFlower(closest);
    }

    private void TryPlantToGarden()
    {
        if (!flowerHolder.HasFlower || currentGarden == null) return;

        GameObject held = flowerHolder.GetHeldFlower();
        GameObject swapped = currentGarden.GetPlantedFlower();

        if (swapped == null)
        {
            // detect any planted child
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

        // plant current
        held.transform.SetParent(currentGarden.transform);
        held.transform.position = currentGarden.GetPlantingPoint().position;

        SproutAndLightManager heldSprout = held.GetComponent<SproutAndLightManager>();
        if (heldSprout != null)
        {
            heldSprout.isPlanted = true;
            heldSprout.isHeld = false;
        }

        Collider2D heldCol = held.GetComponent<Collider2D>();
        if (heldCol != null) heldCol.enabled = true;

        currentGarden.SetPlantedFlower(held);
        flowerHolder.DropFlower();

        // pick up swapped
        if (swapped != null)
            flowerHolder.PickUpFlower(swapped);
    }

    private void HighlightClosestGarden()
    {
        GardenSpot best = null;
        float minDist = float.MaxValue;

        foreach (GardenSpot g in nearbyGardens)
        {
            if (g == null) continue;
            float d = Vector2.Distance(transform.position, g.transform.position);
            if (d < minDist)
            {
                minDist = d;
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
        float minDist = float.MaxValue;

        foreach (GameObject f in nearbyFlowers)
        {
            if (f == null) continue;
            if (f.GetComponent<SproutAndLightManager>() == null) continue;

            float d = Vector2.Distance(transform.position, f.transform.position);
            if (d < minDist)
            {
                minDist = d;
                best = f;
            }
        }
        return best;
    }

    void OnTriggerEnter2D(Collider2D o)
    {
        // Sprouts
        if (o.CompareTag("Sprout") && o.GetComponent<SproutAndLightManager>() != null)
        {
            if (!nearbyFlowers.Contains(o.gameObject))
                nearbyFlowers.Add(o.gameObject);
            return;
        }

        // Gardens
        if (o.CompareTag("Garden"))
        {
            GardenSpot gSpot = o.GetComponent<GardenSpot>();
            if (gSpot != null && !nearbyGardens.Contains(gSpot))
                nearbyGardens.Add(gSpot);
            return;
        }

        // TeapotReceiver (by component)
        if (teapotReceiver == null)
        {
            TeapotReceiver tr = o.GetComponent<TeapotReceiver>();
            if (tr == null && o.attachedRigidbody != null)
                tr = o.attachedRigidbody.GetComponent<TeapotReceiver>();

            if (tr != null)
                teapotReceiver = tr;
        }
    }

    void OnTriggerExit2D(Collider2D o)
    {
        if (o.CompareTag("Sprout"))
        {
            nearbyFlowers.Remove(o.gameObject);
        }
        else if (o.CompareTag("Garden"))
        {
            GardenSpot gSpot = o.GetComponent<GardenSpot>();
            if (gSpot != null && nearbyGardens.Contains(gSpot))
            {
                nearbyGardens.Remove(gSpot);
                if (currentGarden == gSpot)
                {
                    gSpot.SetHighlight(false);
                    currentGarden = null;
                }
            }
        }
        else
        {
            TeapotReceiver tr = o.GetComponent<TeapotReceiver>();
            if (tr == null && o.attachedRigidbody != null)
                tr = o.attachedRigidbody.GetComponent<TeapotReceiver>();

            if (tr == teapotReceiver)
                teapotReceiver = null;
        }
    }
}
