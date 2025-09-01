using UnityEngine;

[RequireComponent(typeof(FlowerHolder))]
public class LooseFlowerPickup : MonoBehaviour
{
    [Tooltip("Radius around Luna or the butterfly to pick up sprouts.")]
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
        _gardenMgr       = GetComponent<FlowerInteractionManager>();
    }

    void Update()
    {
        // 1) Figure out ground vs flight
        bool isGround = (_flowerHolder.holdPoint == _groundHoldPoint);

        // 2) Enable or disable the garden/plant logic accordingly
        if (_gardenMgr != null)
            _gardenMgr.enabled = isGround;

        // 3) Only react to X if not already holding a flower
        if (!Input.GetKeyDown(KeyCode.X) || _flowerHolder.HasFlower)
            return;

        // 4) Determine where to search: on ground around Luna, in flight around butterfly
        Vector3 center = isGround
            ? transform.position
            : (_flyHandler?.butterfly.position ?? transform.position);

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, pickupRadius);

        // 5) If on ground, defer to garden script first, then pick sprouts
        if (isGround)
        {
            foreach (var col in hits)
                if (col.CompareTag("Garden"))
                    return;

            foreach (var col in hits)
                if (col.CompareTag("Sprout"))
                {
                    GameObject go = col.gameObject;
                    go.tag = "Flower";
                    _flowerHolder.PickUpFlower(go);
                    // preserve world scale/rotation on the hold‑point
                    var held = _flowerHolder.GetHeldFlower();
                    held.transform.SetParent(_flowerHolder.holdPoint, true);
                    held.transform.localPosition = Vector3.zero;
                    return;
                }
        }
        // 6) In flight: pick up loose sprouts around the butterfly
        else
        {
            foreach (var col in hits)
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

    void OnDrawGizmosSelected()
    {
        bool isGround = (_flowerHolder != null && _flowerHolder.holdPoint == _groundHoldPoint);
        Vector3 center = isGround
            ? transform.position
            : (FindObjectOfType<ButterflyFlyHandler>()?.butterfly.position ?? transform.position);

        Gizmos.color = isGround ? Color.yellow : Color.cyan;
        Gizmos.DrawWireSphere(center, pickupRadius);
    }
}
