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
        _groundHoldPoint = _flowerHolder.holdPoint;
        _flyHandler      = FindObjectOfType<ButterflyFlyHandler>();
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.X) || _flowerHolder.HasFlower)
            return;

        bool isGround = (_flowerHolder.holdPoint == _groundHoldPoint);
        Collider2D[] hits;

        if (isGround)
        {
            // ground‐only logic unchanged
            hits = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
            foreach (var col in hits)
                if (col.CompareTag("Garden"))
                    return;

            foreach (var col in hits)
            {
                if (col.CompareTag("Sprout"))
                {
                    var go = col.gameObject;
                    go.tag = "Flower";
                    _flowerHolder.PickUpFlower(go);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale    = Vector3.one;
                    return;
                }
            }
        }
        else
        {
            // in‐flight: look for Sprouts around the butterfly, not Luna
            if (_flyHandler?.butterfly == null) return;
            hits = Physics2D.OverlapCircleAll(_flyHandler.butterfly.position, pickupRadius);

            foreach (var col in hits)
            {
                if (col.CompareTag("Sprout"))
                {
                    var go = col.gameObject;
                    go.tag = "Flower";                      // normalize tag
                    _flowerHolder.PickUpFlower(go);         // pick it up
                    go.transform.SetParent(_flowerHolder.holdPoint, false);
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
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }
        else if (_flyHandler?.butterfly != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_flyHandler.butterfly.position, pickupRadius);
        }
    }
}
