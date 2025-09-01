using UnityEngine;

[RequireComponent(typeof(FlowerHolder))]
public class LooseFlowerPickup : MonoBehaviour
{
    [Tooltip("How far away Luna can grab a free‑standing sprout.")]
    public float pickupRadius = 1f;

    [Tooltip("The same Transform you use in ButterflyFlyHandler for groundFlowerHoldPoint")]
    public Transform groundHoldPoint;

    private FlowerHolder _flowerHolder;

    void Awake()
    {
        _flowerHolder = GetComponent<FlowerHolder>();
    }

    void Update()
    {
        // 1) only on the ground (i.e. when FlowerHolder is still using the ground hold point)
        if (_flowerHolder.holdPoint != groundHoldPoint) return;

        // 2) only if player presses X and isn't already holding a flower
        if (!Input.GetKeyDown(KeyCode.X) || _flowerHolder.HasFlower) return;

        // 3) bail out if there's a garden here so your garden script picks first
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
        foreach (var col in hits)
            if (col.CompareTag("Garden"))
                return;

        // 4) otherwise grab the first loose sprout
        foreach (var col in hits)
        {
            if (col.CompareTag("Sprout"))
            {
                // Convert it into a normal flower for the rest of your systems
                var go = col.gameObject;
                go.tag = "Flower";

                // Pick it up
                _flowerHolder.PickUpFlower(go);

                // Immediately zero out its local transform so it sits perfectly
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale    = Vector3.one;
                return;
            }
        }
    }

    // visualize pickup radius
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
