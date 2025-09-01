using UnityEngine;

[RequireComponent(typeof(FlowerHolder))]
public class LooseFlowerPickup : MonoBehaviour
{
    [Tooltip("Radius around Luna or the butterfly to pick up sprouts.")]
    public float pickupRadius = 1f;

    private FlowerHolder        _flowerHolder;
    private Transform           _groundHoldPoint;
    private ButterflyFlyHandler _flyHandler;

    void Awake()
    {
        _flowerHolder    = GetComponent<FlowerHolder>();
        // remember whatever your FlowerHolder was using at start
        _groundHoldPoint = _flowerHolder.holdPoint;
        // find the single flight handler in the scene
        _flyHandler      = FindObjectOfType<ButterflyFlyHandler>();
    }

    void Update()
    {
        // only on X‐press and when not already holding a flower
        if (!Input.GetKeyDown(KeyCode.X) || _flowerHolder.HasFlower)
            return;

        // determine if we’re on the ground or in flight
        bool isGround = (_flowerHolder.holdPoint == _groundHoldPoint);

        // pick the correct center for our search
        Vector3 center = isGround
            ? transform.position
            : (_flyHandler?.butterfly.position ?? transform.position);

        // gather anything within radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, pickupRadius);

        if (isGround)
        {
            // let your garden script go first
            foreach (var col in hits)
                if (col.CompareTag("Garden"))
                    return;

            // otherwise grab a loose sprout
            foreach (var col in hits)
            {
                if (col.CompareTag("Sprout"))
                {
                    var go = col.gameObject;
                    go.tag = "Flower";                     // normalize its tag
                    _flowerHolder.PickUpFlower(go);        // hand it to your holder

                    // reparent with worldPositionStays = true to preserve rotation/scale
                    var held = _flowerHolder.GetHeldFlower();
                    held.transform.SetParent(_flowerHolder.holdPoint, true);
                    held.transform.localPosition = Vector3.zero;
                    return;
                }
            }
        }
        else
        {
            // in‑flight: look for sprouts around the butterfly
            foreach (var col in hits)
            {
                if (col.CompareTag("Sprout"))
                {
                    var go = col.gameObject;
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
        // ground = yellow, flight = cyan
        Gizmos.color = (_flowerHolder != null && _flowerHolder.holdPoint == _groundHoldPoint)
            ? Color.yellow
            : Color.cyan;

        Vector3 center = (_flowerHolder != null && _flowerHolder.holdPoint == _groundHoldPoint)
            ? transform.position
            : (_flyHandler?.butterfly.position ?? transform.position);

        Gizmos.DrawWireSphere(center, pickupRadius);
    }
}
