using UnityEngine;

[RequireComponent(typeof(FlowerHolder), typeof(SpriteRenderer))]
public class LooseFlowerPickup : MonoBehaviour
{
    [Tooltip("How far away Luna can grab a free‑standing sprout.")]
    public float pickupRadius = 1f;

    private FlowerHolder   _flowerHolder;
    private SpriteRenderer _sprLuna;

    void Awake()
    {
        _flowerHolder = GetComponent<FlowerHolder>();
        _sprLuna      = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // if Luna's ground sprite is hidden, we must be in flight → do nothing
        if (!_sprLuna.enabled) return;

        // only pick up when pressing X and not already holding anything
        if (Input.GetKeyDown(KeyCode.X) && !_flowerHolder.HasFlower)
        {
            // first, bail if you're standing over a garden so that
            // FlowerInteractionManager can handle it
            Collider2D[] allHits = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
            foreach (var col in allHits)
                if (col.CompareTag("Garden"))
                    return;

            // otherwise, look for any loose sprouts
            foreach (var col in allHits)
            {
                if (col.CompareTag("Sprout"))
                {
                    // convert it into a normal flower so downstream code sees it
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
