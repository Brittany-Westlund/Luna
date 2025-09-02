using UnityEngine;

[RequireComponent(typeof(FlowerHolder))]
public class LooseFlowerPickup : MonoBehaviour
{
    [Tooltip("How far Luna or the butterfly can reach to pick up free sprouts.")]
    public float pickupRadius = 1f;

    private FlowerHolder             _holder;
    private Transform                _groundHoldPoint;
    private ButterflyFlyHandler      _flyHandler;
    private FlowerInteractionManager _gardenMgr;
    private GroundProximityLimiter   _proximityLimiter;

    void Awake()
    {
        _holder          = GetComponent<FlowerHolder>();
        _groundHoldPoint = _holder.holdPoint;
        _flyHandler      = FindObjectOfType<ButterflyFlyHandler>();
        _gardenMgr       = GetComponent<FlowerInteractionManager>();

        // look for your limiter on the butterfly transform
        if (_flyHandler != null && _flyHandler.butterfly != null)
            _proximityLimiter = _flyHandler.butterfly.GetComponent<GroundProximityLimiter>();

        if (_proximityLimiter == null)
            Debug.LogWarning("LooseFlowerPickup: No GroundProximityLimiter found on the butterfly!");
    }

    void Update()
    {
        bool isGround = (_holder.holdPoint == _groundHoldPoint);

        // 0) Toggle the ground‑limiter
        if (_proximityLimiter != null)
        {
            bool shouldBeOn = !isGround;
            if (_proximityLimiter.enabled != shouldBeOn)
            {
                _proximityLimiter.enabled = shouldBeOn;
                // Debug.Log($"GroundLimiter.enabled = {shouldBeOn}");
            }
        }

        // 1) Disable garden‑planting when in flight
        if (_gardenMgr != null)
            _gardenMgr.enabled = isGround;

        // 2) Only fire on X if not already holding
        if (!Input.GetKeyDown(KeyCode.X) || _holder.HasFlower)
            return;

        // 3) Choose search center
        Vector3 center = isGround
            ? transform.position
            : (_flyHandler?.butterfly.position ?? transform.position);

        // 4) Find any sprouts
        var hits = Physics2D.OverlapCircleAll(center, pickupRadius);

        if (isGround)
        {
            // let gardens go first
            foreach (var col in hits)
                if (col.CompareTag("Garden"))
                    return;
        }

        // 5) Pickup the first sprout
        foreach (var col in hits)
        {
            if (col.CompareTag("Sprout"))
            {
                Pickup(col.gameObject);
                return;
            }
        }
    }

    private void Pickup(GameObject go)
    {
        // hand off to FlowerHolder
        _holder.PickUpFlower(go);

        // reparent under the active holdPoint, preserving world‐space
        var heldT = _holder.GetHeldFlower().transform;
        heldT.SetParent(_holder.holdPoint, true);
        heldT.localPosition = Vector3.zero;
    }

    void OnDrawGizmosSelected()
    {
        bool isGround = (_holder != null && _holder.holdPoint == _groundHoldPoint);
        Vector3 center = isGround
            ? transform.position
            : (FindObjectOfType<ButterflyFlyHandler>()?.butterfly.position ?? transform.position);

        Gizmos.color = isGround ? Color.yellow : Color.cyan;
        Gizmos.DrawWireSphere(center, pickupRadius);
    }
}
