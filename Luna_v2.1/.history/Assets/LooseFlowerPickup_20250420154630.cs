using UnityEngine;

[RequireComponent(typeof(FlowerHolder))]
public class LooseFlowerPickup : MonoBehaviour
{
    [Tooltip("Radius around Luna or the butterfly to pick up items.")]
    public float pickupRadius = 1f;

    private FlowerHolder            _flowerHolder;
    private Transform               _groundHoldPoint;
    private ButterflyFlyHandler     _flyHandler;

    void Awake()
    {
        _flowerHolder    = GetComponent<FlowerHolder>();
        // Remember the initial holdPoint as 'ground'
        _groundHoldPoint = _flowerHolder.holdPoint;
        // Grab the one-and-only flight handler in the scene
        _flyHandler      = FindObjectOfType<ButterflyFlyHandler>();
    }

    void Update()
    {
        // Only act on X presses when we're not already carrying a flower
        if (!Input.GetKeyDown(KeyCode.X) || _flowerHolder.HasFlower)
            return;

        // Are we still on the ground?
        bool isGround = (_flowerHolder.holdPoint == _groundHoldPoint);

        if (isGround)
        {
            // —— Ground pickup (sprouts + garden) —— 

            // 1) Let the garden script go first
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
            foreach (var col in hits)
                if (col.CompareTag("Garden"))
                    return;

            // 2) Otherwise pick up the first loose sprout
            foreach (var col in hits)
            {
                if (col.CompareTag("Sprout"))
                {
                    var go = col.gameObject;
                    // Convert to a normal flower
                    go.tag = "Flower";
                    _flowerHolder.PickUpFlower(go);
                    // Snap it exactly into the hold point
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale    = Vector3.one;
                    return;
                }
            }
        }
        else
        {
            // —— In‑flight pickup (flowers only) ——

            // need the flight handler and its butterfly reference
            if (_flyHandler == null || _flyHandler.butterfly == null)
                return;

            // look around the butterfly, not Luna
            Vector3 center = _flyHandler.butterfly.position;
            Collider2D[] hits = Physics2D.OverlapCircleAll(center, pickupRadius);
            foreach (var col in hits)
            {
                // pick up any free-floating flower
                if (col.CompareTag("Flower"))
                {
                    var go = col.gameObject;
                    _flowerHolder.PickUpFlower(go);
                    // snap it into the flight hold point
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale    = Vector3.one;
                    return;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (_flowerHolder != null && _flowerHolder.holdPoint == _groundHoldPoint)
        {
            // ground radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }
        else if (_flyHandler != null && _flyHandler.butterfly != null)
        {
            // flight radius
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_flyHandler.butterfly.position, pickupRadius);
        }
    }
}
