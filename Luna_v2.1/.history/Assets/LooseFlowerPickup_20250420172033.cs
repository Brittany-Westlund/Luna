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

        // grab the limiter off the butterfly transform
        if (_flyHandler != null && _flyHandler.butterfly != null)
            _proximityLimiter = _flyHandler.butterfly.GetComponent<GroundProximityLimiter>();
    }

    void Update()
    {
        bool isGround = (_holder.holdPoint == _groundHoldPoint);

        // —— 0) Toggle the ground‑limiter: off on ground, on in flight —— 
        if (_proximityLimiter != null)
            _proximityLimiter.enabled = !isGround;

        // —— 1) Enable/disable garden‐planting script based on ground vs flight —— 
        if (_gardenMgr != null)
            _gardenMgr.enabled = isGround;

        // —— 2) Only handle pickup on X if not already holding a flower —— 
        if (!Input.GetKeyDown(KeyCode.X) || _holder.HasFlower)
            return;

        // —— 3) Choose search center —— 
        Vector3 center = isGround
            ? transform.position
            : (_flyHandler?.butterfly.position ?? transform.position);

        // —— 4) Gather any sprouts in range —— 
        var hits = Physics2D.OverlapCircleAll(center, pickupRadius);

        if (isGround)
        {
            // let garden script take priority
            foreach (var col in hits)
                if (col.CompareTag("Garden"))
                    return;
        }

        // —— 5) Pick the first loose sprout —— 
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
        // retag and hand off to your FlowerHolder
        go.tag = "Flower";
        _holder.PickUpFlower(go);

        // parent under the active holdPoint, preserving world transform
        var held = _holder.GetHeldFlower();
        held.transform.SetParent(_holder.holdPoint, true);
        held.transform.localPosition = Vector3.zero;
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
