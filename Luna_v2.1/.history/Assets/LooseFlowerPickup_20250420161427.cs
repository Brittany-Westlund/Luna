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
        _gardenMgr       = GetComponent<FlowerInteractionManager>();
    }

    void Update()
    {
        // ———————————————————————————————
        // 1) Toggle garden‐planting on/off
        bool isGround = (_flowerHolder.holdPoint == _groundHoldPoint);
        if (_gardenMgr != null)
            _gardenMgr.enabled = isGround;
        // ———————————————————————————————

        // Only handle pickup when pressing X and not already holding something
        if (!Input.GetKeyDown(KeyCode.X) || _flowerHolder.HasFlower)
            return;

        // Decide whose position to sample: Luna if ground, butterfly if flying
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

            // Then grab loose sprouts
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
        else
        {
            // In flight, pick up loose sprouts around the butterfly
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
        bool isGround = (_flowerHolder != null && _flowerHolder.holdPoint == _groundHoldPoint);
        Vector3 center = isGround
            ? transform.position
            : (_flyHandler?.butterfly.position ?? transform.position);

        Gizmos.color = isGround ? Color.yellow : Color.cyan;
        Gizmos.DrawWireSphere(center, pickupRadius);
    }
}
