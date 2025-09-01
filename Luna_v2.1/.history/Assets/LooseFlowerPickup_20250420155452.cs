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
        Vector3 center = isGround 
            ? transform.position 
            : (_flyHandler?.butterfly.position ?? transform.position);

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, pickupRadius);

        if (isGround)
        {
            // garden-first
            foreach (var col in hits)
                if (col.CompareTag("Garden"))
                    return;

            // ground sprouts
            foreach (var col in hits)
            {
                if (col.CompareTag("Sprout"))
                {
                    GameObject go = col.gameObject;
                    // save its current world rotation
                    Quaternion rot = go.transform.rotation;
                    go.tag = "Flower";
                    _flowerHolder.PickUpFlower(go);
                    // restore rotation so it doesn't spin
                    go.transform.rotation = rot;
                    return;
                }
            }
        }
        else
        {
            // in-flight sprouts
            foreach (var col in hits)
            {
                if (col.CompareTag("Sprout"))
                {
                    GameObject go = col.gameObject;
                    Quaternion rot = go.transform.rotation;
                    go.tag = "Flower";
                    _flowerHolder.PickUpFlower(go);
                    go.transform.rotation = rot;
                    return;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
        if (_flyHandler?.butterfly != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_flyHandler.butterfly.position, pickupRadius);
        }
    }
}
