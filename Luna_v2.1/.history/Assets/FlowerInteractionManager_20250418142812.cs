// FlowerInteractionManager.cs
using UnityEngine;

[RequireComponent(typeof(FlowerHolder))]
public class FlowerInteractionManager : MonoBehaviour
{
    [Header("Interaction Radius")]
    [Tooltip("How far around Luna to detect sprouts, gardens, and teapots.")]
    public float interactRadius = 1.5f;

    private FlowerHolder flowerHolder;
    private GardenSpot   currentGarden;
    private TeapotReceiver currentTeapot;

    void Awake()
    {
        flowerHolder = GetComponent<FlowerHolder>();
    }

    void Update()
    {
        // 1) Teapot first
        TeapotReceiver pot = FindNearestTeapot();
        currentTeapot = pot;

        if (pot != null && Input.GetKeyDown(KeyCode.X))
        {
            if (flowerHolder.HasFlower)
            {
                pot.AddFlowerToTeapot(flowerHolder);
            }
            else if (pot.HasAnyIngredients())
            {
                pot.RetrieveLastFlower(flowerHolder);
            }
            return;
        }

        // 2) Otherwise, garden pickup/plant
        HighlightNearestGarden();

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
        GameObject sproutGO = FindNearestWithTag("Sprout");
        if (sproutGO == null) return;

        var spr = sproutGO.GetComponent<SproutAndLightManager>();
        if (spr == null) return;

        // clear old spot if it was planted
        if (spr.isPlanted && sproutGO.transform.parent != null)
        {
            var oldSpot = sproutGO.transform.parent.GetComponent<GardenSpot>();
            if (oldSpot != null) oldSpot.ClearPlantedFlower();
        }

        flowerHolder.PickUpFlower(sproutGO);
    }

    private void TryPlantToGarden()
    {
        if (!flowerHolder.HasFlower || currentGarden == null) return;

        GameObject held = flowerHolder.GetHeldFlower();
        GameObject old  = currentGarden.GetPlantedFlower();

        // only swap if that GardenSpot really held this flower
        if (old == held) old = null;

        currentGarden.ClearPlantedFlower();

        // plant
        held.transform.SetParent(currentGarden.transform);
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
    }

    private void HighlightNearestGarden()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius);
        GardenSpot best = null;
        float bestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Garden")) continue;
            var gs = hit.GetComponent<GardenSpot>();
            if (gs == null) continue;
            float d = Vector2.Distance(transform.position, gs.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = gs;
            }
        }

        if (best != currentGarden)
        {
            if (currentGarden != null) currentGarden.SetHighlight(false);
            currentGarden = best;
            if (currentGarden != null) currentGarden.SetHighlight(true);
        }
    }

    private TeapotReceiver FindNearestTeapot()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius);
        TeapotReceiver best = null;
        float bestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Teapot")) continue;
            var tr = hit.GetComponent<TeapotReceiver>();
            if (tr == null) continue;
            float d = Vector2.Distance(transform.position, hit.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = tr;
            }
        }
        return best;
    }

    private GameObject FindNearestWithTag(string tag)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius);
        GameObject best = null;
        float bestDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (!hit.CompareTag(tag)) continue;
            var go = hit.gameObject;
            float d = Vector2.Distance(transform.position, go.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = go;
            }
        }
        return best;
    }

    // visualize radius
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
