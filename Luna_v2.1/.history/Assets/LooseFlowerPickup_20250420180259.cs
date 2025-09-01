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

    void Awake()
    {
        _holder          = GetComponent<FlowerHolder>();
        _groundHoldPoint = _holder.holdPoint;
        _flyHandler      = FindObjectOfType<ButterflyFlyHandler>();
        _gardenMgr       = GetComponent<FlowerInteractionManager>();
    }

    void Update()
    {
        bool isGround = (_holder.holdPoint == _groundHoldPoint);

        // 1) Disable garden‐planting while flying
        if (_gardenMgr != null)
            _gardenMgr.enabled = isGround;

        // 2) Only react on X if not already holding a flower
        if (!Input.GetKeyDown(KeyCode.X) || _holder.HasFlower)
            return;

        // 3) Choose search center: Luna on ground, butterfly in flight
        Vector3 center = isGround
            ? transform.position
            : (_flyHandler?.butterfly.position ?? transform.position);

        // 4) Find any loose sprout in range
        var hits = Physics2D.OverlapCircleAll(center, pickupRadius);

        // On ground: let the garden script take priority
        if (isGround)
        {
            foreach (var col in hits)
                if (col.CompareTag("Garden"))
                    return;
        }

        // 5) Pick up the first “Sprout” we find
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
        // 1) Hand off to FlowerHolder
        _holder.PickUpFlower(go);

        // 2) Parent under the active holdPoint, preserving world space
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
