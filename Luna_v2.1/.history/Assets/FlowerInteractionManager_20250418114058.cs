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
        // If we’re in teapot range, let the teapot handle all X presses
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

        SproutAndLightManager sprout = closest.GetComponent<SproutAndLightManager>();
        if (sprout == null) return;

        if (sprout.isPlanted && closest.transform.parent != null)
        {
            GardenSpot oldSpot = closest.transform.parent.GetComponent<GardenSpot>();
            if (oldSpot != null)
                oldSpot.ClearPlantedFlower();
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
            // check for any child sprout
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

        // plant held flower
        heldFlower.transform.SetParent(currentGarden.transform);
        heldFlower.transform.position = currentGarden.GetPlantingPoint().position;

        SproutAndLightManager sm = heldFlower.GetComponent<SproutAndLightManager>();
        if (sm != null)
        {
            sm.isPlanted = true;
            sm.isHeld    = false;
        }

        Collider2D col = heldFlower.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        currentGarden.SetPlantedFlower(heldFlower);
        flowerHolder.DropFlower();

        // pick up swapped flower if there was one
        if (swapped != null)
            flowerHolder.PickUpFlower(swapped);
    }

    private void HighlightClosestGarden()
    {
        GardenSpot best     = null;
        float      bestDist = float.MaxValue;

        foreach (GardenSpot spot in nearbyGardens)
        {
            if (spot == null) continue;
            float d = Vector2.Distance(transform.position, spot.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best     = spot;
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
        GameObject best     = null;
        float      bestDist = float.MaxValue;

        foreach (GameObject f in nearbyFlowers)
        {
            if (f == null) continue;
            if (f.GetComponent<SproutAndLightManager>() == null) continue;

            float d = Vector2.Distance(transform.position, f.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best     = f;
            }
        }

        return best;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // pick up loose flowers
        if (other.CompareTag("Sprout") && other.GetComponent<SproutAndLightManager>() != null)
        {
            if (!nearbyFlowers.Contains(other.gameObject))
                nearbyFlowers.Add(other.gameObject);
            return;
        }

        // highlight gardens
        if (other.CompareTag("Garden"))
        {
            GardenSpot gs = other.GetComponent<GardenSpot>();
            if (gs != null && !nearbyGardens.Contains(gs))
                nearbyGardens.Add(gs);
            return;
        }

        // detect teapot
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
        {
            nearbyFlowers.Remove(other.gameObject);
        }
        else if (other.CompareTag("Garden"))
        {
            GardenSpot gs = other.GetComponent<GardenSpot>();
            if (gs != null && nearbyGardens.Remove(gs) && currentGarden == gs)
            {
                gs.SetHighlight(false);
                currentGarden = null;
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
