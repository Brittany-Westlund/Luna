using UnityEngine;

[RequireComponent(typeof(FlowerHolder))]
public class LooseFlowerPickup : MonoBehaviour
{
    [Tooltip("How far away Luna can grab a free‑standing sprout on the ground.")]
    public float pickupRadius = 1f;

    private FlowerHolder _flowerHolder;
    private Transform    _groundHoldPoint;

    void Awake()
    {
        _flowerHolder    = GetComponent<FlowerHolder>();
        // remember whatever the holder was using at start (your ground point)
        _groundHoldPoint = _flowerHolder.holdPoint;
    }

    void Update()
    {
        // 1) Only on the ground
        if (_flowerHolder.holdPoint != _groundHoldPoint)
            return;

        // 2) Only when pressing X and not already holding a flower
        if (!Input.GetKeyDown(KeyCode.X) || _flowerHolder.HasFlower)
            return;

        // 3) Let the garden script handle beds first
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
        foreach (var col in hits)
        {
            if (col.CompareTag("Garden"))
                return;
        }

        // 4) Otherwise pick up the first loose sprout
        foreach (var col in hits)
        {
            if (col.CompareTag("Sprout"))
            {
                GameObject go = col.gameObject;
                // convert into a normal flower for downstream code
                go.tag = "Flower";

                // hand it off
                _flowerHolder.PickUpFlower(go);

                // snap into place
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale    = Vector3.one;
                return;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // visualize pickup range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
