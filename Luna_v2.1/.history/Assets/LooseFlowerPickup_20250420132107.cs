using UnityEngine;

[RequireComponent(typeof(FlowerHolder))]
public class LooseFlowerPickup : MonoBehaviour
{
    [Tooltip("How far away Luna can grab a free‑standing flower.")]
    public float pickupRadius = 1f;

    private FlowerHolder _flowerHolder;

    void Awake()
    {
        _flowerHolder = GetComponent<FlowerHolder>();
    }

    void Update()
    {
        // Only try to pick up if X is pressed, Luna is on the ground, and not already holding one:
        if (Input.GetKeyDown(KeyCode.X) && !_flowerHolder.HasFlower)
        {
            // first see if FlowerInteractionManager would do a garden pickup…
            // if it's already handling a garden (you'll still be 'inside' that manager's trigger),
            // your new script shouldn't steal that, so bail out if they just picked a garden flower.
            // But because FlowerInteractionManager only picks from a garden when currentGarden != null,
            // we can guard against it by checking for any nearby garden first:

            // (optional) skip if a garden is close enough
            Collider2D[] gardenHits = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
            foreach (var g in gardenHits)
                if (g.CompareTag("Garden"))
                    return;

            // now look for any free flower
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
            foreach (var col in hits)
            {
                if (col.CompareTag("Flower"))
                {
                    _flowerHolder.PickUpFlower(col.gameObject);
                    return;
                }
            }
        }
    }

    // visualize the radius in the editor (optional)
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
