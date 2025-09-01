using UnityEngine;

[RequireComponent(typeof(FlowerHolder))]
public class LooseFlowerPickup : MonoBehaviour
{
    [Tooltip("Radius around Luna or the butterfly to pick up sprouts or flowers.")]
    public float pickupRadius = 1f;

    private FlowerHolder              _flowerHolder;
    private Transform                 _groundHoldPoint;
    private ButterflyFlyHandler       _flyHandler;
    private FollowAndFlip             _followAndFlip;   // <-- declared here
    private FlowerInteractionManager  _gardenMgr;
    private bool                      _wasGround = true; // <-- and here

    void Awake()
    {
        _flowerHolder    = GetComponent<FlowerHolder>();
        _groundHoldPoint = _flowerHolder.holdPoint;
        _flyHandler      = FindObjectOfType<ButterflyFlyHandler>();

        if (_flyHandler != null)
            _followAndFlip = _flyHandler.GetComponent<FollowAndFlip>();

        _gardenMgr       = GetComponent<FlowerInteractionManager>();
    }

    void Update()
    {
        // ——————————————————————
        // 1) Detect mount transition and reset follower
        bool isGround = (_flowerHolder.holdPoint == _groundHoldPoint);
        if (_wasGround && !isGround && _followAndFlip != null)
        {
            _followAndFlip.StopAllCoroutines();
            _followAndFlip.SetFollow(false);
        }
        _wasGround = isGround;
        // ——————————————————————

        // 2) Toggle garden‐planting logic on/off
        if (_gardenMgr != null)
            _gardenMgr.enabled = isGround;

        // 3) Only handle pickup on X if not already holding
        if (!Input.GetKeyDown(KeyCode.X) || _flowerHolder.HasFlower)
            return;

        // 4) Choose center: Luna on ground, butterfly in flight
        Vector3 center = isGround
            ? transform.position
            : (_flyHandler?.butterfly.position ?? transform.position);

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, pickupRadius);

        if (isGround)
        {
            // ground: garden‑first, then loose sprouts
            foreach (var col in hits)
                if (col.CompareTag("Garden"))
                    return;

            foreach (var col in hits)
                if (col.CompareTag("Sprout"))
                    Pickup(col.gameObject);
        }
        else
        {
            // flight: pick up loose sprouts around butterfly
            foreach (var col in hits)
                if (col.CompareTag("Sprout"))
                    Pickup(col.gameObject);
        }
    }

    void Pickup(GameObject go)
    {
        go.tag = "Flower";
        _flowerHolder.PickUpFlower(go);
        var held = _flowerHolder.GetHeldFlower();
        held.transform.SetParent(_flowerHolder.holdPoint, true);
        held.transform.localPosition = Vector3.zero;
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
