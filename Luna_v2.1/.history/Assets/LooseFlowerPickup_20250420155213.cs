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
        // remember the holder’s original (ground) point
        _groundHoldPoint = _flowerHolder.holdPoint;
        // grab your flight handler so we know where the butterfly is in‑air
        _flyHandler      = FindObjectOfType<ButterflyFlyHandler>();
    }

    void Update()
    {
        // only fire on X‐press when not already holding something
        if (!Input.GetKeyDown(KeyCode.X) || _flowerHolder.HasFlower)
            return;

        bool isGround = (_flowerHolder.holdPoint == _groundHoldPoint);

        // gather the relevant center and hits
        Vector3 center = isGround
            ? transform.position
            : _flyHandler?.butterfly.position ?? transform.position;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, pickupRadius);

        if (isGround)
        {
            // 1) let the garden script go first
            foreach (var col in hits)
                if (col.CompareTag("Garden"))
                    return;

            // 2) otherwise grab the first loose sprout
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
            // in‑flight: pick up any loose sprout tagged "Sprout"
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
    }

    void OnDrawGizmosSelected()
    {
        // visualizes ground vs flight pickup radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);

        if (_flyHandler?.butterfly != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_flyHandler.butterfly.position, pickupRadius);
        }
    }
}
