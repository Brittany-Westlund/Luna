using UnityEngine;

[RequireComponent(typeof(FlowerHolder))]
public class LooseFlowerPickup : MonoBehaviour
{
    [Tooltip("Radius around Luna or the butterfly to pick up items.")]
    public float pickupRadius = 1f;

    private FlowerHolder        _flowerHolder;
    private Transform           _groundHoldPoint;
    private ButterflyFlyHandler _flyHandler;

    void Awake()
    {
        _flowerHolder    = GetComponent<FlowerHolder>();
        // Remember the holder’s original ground hold‑point
        _groundHoldPoint = _flowerHolder.holdPoint;
        // Find the butterfly’s flight handler for in‑air pickups
        _flyHandler      = FindObjectOfType<ButterflyFlyHandler>();
    }

    void Update()
    {
        // Only on X press, and only if Luna isn’t already holding a flower
        if (!Input.GetKeyDown(KeyCode.X) || _flowerHolder.HasFlower)
            return;

        bool isGround = (_flowerHolder.holdPoint == _groundHoldPoint);
        Vector3 center = isGround
            ? transform.position
            : (_flyHandler?.butterfly.position ?? transform.position);

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, pickupRadius);

        if (isGround)
        {
            // Ground logic: let garden script handle beds first
            foreach (var col in hits)
                if (col.CompareTag("Garden"))
                    return;

            // Then pick up the first loose sprout
            foreach (var col in hits)
            {
                if (col.CompareTag("Sprout"))
                {
                    GameObject go = col.gameObject;
                    go.tag = "Flower";
                    _flowerHolder.PickUpFlower(go);
                    return;
                }
            }
        }
        else
        {
            // In‑flight logic: pick up any loose sprout without spinning
            foreach (var col in hits)
            {
                if (col.CompareTag("Sprout"))
                {
                    GameObject go = col.gameObject;
                    go.tag = "Flower";

                    // Disable its physics so it won't spin
                    var cd = go.GetComponent<Collider2D>();
                    if (cd) cd.enabled = false;
                    var rb = go.GetComponent<Rigidbody2D>();
                    if (rb) rb.simulated = false;

                    _flowerHolder.PickUpFlower(go);
                    return;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualize pickup radius on ground vs flight
        Gizmos.color = (_flowerHolder != null && _flowerHolder.holdPoint == _groundHoldPoint)
            ? Color.yellow
            : Color.cyan;

        Vector3 center = (_flowerHolder != null && _flowerHolder.holdPoint == _groundHoldPoint)
            ? transform.position
            : (_flyHandler?.butterfly.position ?? transform.position);

        Gizmos.DrawWireSphere(center, pickupRadius);
    }
}
