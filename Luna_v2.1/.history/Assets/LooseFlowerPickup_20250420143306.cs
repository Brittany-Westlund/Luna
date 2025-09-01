using UnityEngine;

[RequireComponent(typeof(FlowerHolder), typeof(FollowAndFlip))]
public class LooseFlowerPickup : MonoBehaviour
{
    [Tooltip("How far away Luna can grab a free‑standing sprout.")]
    public float pickupRadius = 1f;

    private FlowerHolder   _flowerHolder;
    private FollowAndFlip  _followAndFlip;

    void Awake()
    {
        _flowerHolder    = GetComponent<FlowerHolder>();
        _followAndFlip   = GetComponent<FollowAndFlip>();
    }

    void Update()
    {
        // ONLY run when on the ground (FollowAndFlip enabled)
        if (!_followAndFlip.enabled) return;

        // Only react to X when not already holding a flower
        if (Input.GetKeyDown(KeyCode.X) && !_flowerHolder.HasFlower)
        {
            // 1) If standing over a garden, bail so your garden script handles it
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
            foreach (var col in hits)
                if (col.CompareTag("Garden"))
                    return;

            // 2) Otherwise pick up the first loose sprout
            foreach (var col in hits)
            {
                if (col.CompareTag("Sprout"))
                {
                    // convert to a normal flower so everything downstream sees it
                    col.gameObject.tag = "Flower";
                    _flowerHolder.PickUpFlower(col.gameObject);
                    return;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
