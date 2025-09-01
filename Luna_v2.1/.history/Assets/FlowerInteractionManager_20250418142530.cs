// FlowerInteractionManager.cs
using UnityEngine;

[RequireComponent(typeof(FlowerHolder))]
public class FlowerInteractionManager : MonoBehaviour
{
    [Header("Ranges & Layers")]
    public float interactRadius = 1.5f;
    public LayerMask flowerLayer;
    public LayerMask gardenLayer;
    public LayerMask teapotLayer;

    private FlowerHolder flowerHolder;
    private GardenSpot   currentGarden;
    private TeapotReceiver currentTeapot;

    void Awake()
    {
        flowerHolder = GetComponent<FlowerHolder>();
    }

    void Update()
    {
        // 1) Always check for teapot first
        TeapotReceiver pot = FindNearest<TeapotReceiver>(teapotLayer);
        currentTeapot = pot;
        if (pot != null && Input.GetKeyDown(KeyCode.X))
        {
            // retrieve first if empty and has ingredients
            if (!flowerHolder.HasFlower && pot.HasAnyIngredients())
                pot.RetrieveLastFlower(flowerHolder);
            else if (flowerHolder.HasFlower)
                pot.AddFlowerToTeapot(flowerHolder);
            return;
        }

        // 2) Otherwise garden pickup/plant
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
        // just pick up the nearest SPRITE in flowerLayer
        Collider2D c = Physics2D.OverlapCircle(transform.position, interactRadius, flowerLayer);
        if (c == null) return;
        GameObject flower = c.gameObject;
        var spr = flower.GetComponent<SproutAndLightManager>();
        if (spr == null) return;

        // clear if it was planted
        if (spr.isPlanted && flower.transform.parent != null)
        {
            var spot = flower.transform.parent.GetComponent<GardenSpot>();
            if (spot != null) spot.ClearPlantedFlower();
        }

        flowerHolder.PickUpFlower(flower);
    }

    private void TryPlantToGarden()
    {
        if (currentGarden == null || !flowerHolder.HasFlower) return;

        GameObject held  = flowerHolder.GetHeldFlower();
        GameObject old   = currentGarden.GetPlantedFlower();

        // if the spot really had one, we’ll swap; otherwise old stays null
        if (old == held) old = null;

        currentGarden.ClearPlantedFlower();

        // plant held
        held.transform.SetParent(currentGarden.transform);
        held.transform.position = currentGarden.GetPlantingPoint().position;
        var sm = held.GetComponent<SproutAndLightManager>();
        if (sm != null) { sm.isPlanted = true; sm.isHeld = false; }
        var col = held.GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
        currentGarden.SetPlantedFlower(held);
        flowerHolder.DropFlower();

        // swap back old if it existed
        if (old != null)
            flowerHolder.PickUpFlower(old);
    }

    private void HighlightNearestGarden()
    {
        // find all gardens in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius, gardenLayer);
        GardenSpot best = null;
        float bestDist = float.MaxValue;
        foreach (var h in hits)
        {
            GardenSpot gs = h.GetComponent<GardenSpot>();
            if (gs == null) continue;
            float d = Vector2.Distance(transform.position, gs.transform.position);
            if (d < bestDist) { bestDist = d; best = gs; }
        }

        // toggle highlights
        if (best != currentGarden)
        {
            if (currentGarden != null) currentGarden.SetHighlight(false);
            currentGarden = best;
            if (currentGarden != null) currentGarden.SetHighlight(true);
        }
    }

    // Generic helper: finds the nearest component T in a layer within interactRadius
    private T FindNearest<T>(LayerMask layer) where T: Component
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRadius, layer);
        T best = null;
        float bestDist = float.MaxValue;
        foreach (var h in hits)
        {
            T comp = h.GetComponent<T>() ?? h.attachedRigidbody?.GetComponent<T>();
            if (comp == null) continue;
            float d = Vector2.Distance(transform.position, h.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = comp;
            }
        }
        return best;
    }

    // (Optional) draw your interact circle in the editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
