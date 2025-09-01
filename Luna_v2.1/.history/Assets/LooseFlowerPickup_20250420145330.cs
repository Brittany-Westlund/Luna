using UnityEngine;

[RequireComponent(typeof(FlowerHolder))]
public class LooseFlowerPickup : MonoBehaviour
{
    [Tooltip("How far away Luna can grab a free‑standing sprout.")]
    public float pickupRadius = 1f;

    private FlowerHolder _flowerHolder;
    private Transform    _groundHoldPoint;

    void Start()
    {
        // grab your FlowerHolder and remember its initial holdPoint (the ground one)
        _flowerHolder   = GetComponent<FlowerHolder>();
        _groundHoldPoint = _flowerHolder.holdPoint;
    }

    void Update()
    {
        // Only work if we're using the ground holdPoint (i.e. not flying)
        if (_flowerHolder.holdPoint != _groundHoldPoint) return;

        // Only on X press when not already holding a flower
        if (!Input.GetKeyDown(KeyCode.X) || _flowerHolder.HasFlower) return;

        // 1) If over any garden, bail out so the garden script takes priority
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
        foreach (var col in hits)
            if (col.CompareTag("Garden"))
                return;

        // 2) Otherwise pick up the first loose "Sprout"
        foreach (var col in hits)
        {
            if (col.CompareTag("Sprout"))
            {
                GameObject go = col.gameObject;
                // convert it to a normal Flower for all downstream systems
                go.tag = "Flower";
                
                // hand it off
                _flowerHolder.PickUpFlower(go);

                // zero out its local transform so it sits perfectly in your holdPoint
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale    = Vector3.one;
                return;
            }
        }
    }

    // visualize your pickup radius in the Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
