using UnityEngine;

[RequireComponent(typeof(FlowerHolder))]
public class LooseFlowerPickup : MonoBehaviour
{
    [Tooltip("Radius around Luna or the butterfly to pick up sprouts or flowers.")]
    public float pickupRadius = 1f;

    private FlowerHolder              _flowerHolder;
    private Transform                 _groundHoldPoint;
    private ButterflyFlyHandler       _flyHandler;
    private FlowerInteractionManager  _gardenMgr;

    void Awake()
    {
        _flowerHolder    = GetComponent<FlowerHolder>();
        _groundHoldPoint = _flowerHolder.holdPoint;
        _flyHandler      = FindObjectOfType<ButterflyFlyHandler>();
        // grab the garden/planting manager on the same object (if present)
        _gardenMgr       = GetComponent<FlowerInteractionManager>();
    }

    void Update()
    {
        // Figure out if we're on the ground or flying
        bool isGround = (_flowerHolder.holdPoint == _groundHoldPoint);

        // Toggle the garden/planting logic on or off
        if (_gardenMgr != null)
            _gardenMgr.enabled = isGround;

        // Only handle pickup when pressing X and not already holding something
        if (!Input.GetKeyDown(KeyCode.X) || _flowerHolder.HasFlower)
            return;

        Vector3 center = isGround
            ? transform.position
            : (_flyHandler?.butterfly.position ?? transform.position);

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, pickupRadius);

        if (isGround)
        {
            // On ground, let the garden script pick first
            foreach (var col in hits)
                if (col.CompareTag("Garden"))
                    return;

            // Then grab sprouts
            foreach (var col in hits)
            {
                if (col.CompareTag("Sprout"))
                {
                    GameObject go = col.gameObject;
                    go.tag = "Flower";
                    _flowerHolder.PickUpFlower(go);
                    // preserve world rotation/scale
                    var held = _flowerHolder.GetHeldFlower();
                    held.transform.SetParent(_flowerHolder.holdPoint, true);
                    held.transform.localPosition = Vector3.zero;
                    return;
                }
            }
        }
        else
        {
            // In flight, grab loose sprouts around the butterfly
            foreach (var col in hits)
            {
                if (col.CompareTag("Sprout"))
                {
                    GameObject go = col.gameObject;
                    go.tag = "Flower";
                    _flowerHolder.PickUpFlower(go);
                    var held = _flowerHolder.GetHeldFlower();
                    held.transform.SetParent(_flowerHolder.holdPoint, true);
                    held.transform.localPosition = Vector3.zero;
                    return;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        bool isGround = (_flowerHolder != null && _flowerHolder.holdPoint == _groundHoldPoint);
        Vector3 center = isGround
            ? transform.position
            : (_flyHandler?.butterfly.position ?? transform.position);

        Gizmos.color = isGround ? Color.yellow : Color.cyan;
        Gizmos.DrawWireSphere(center, pickupRadius);
    }
}
