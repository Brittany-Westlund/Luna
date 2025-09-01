using UnityEngine;

[RequireComponent(typeof(FlowerHolder))]
public class LooseFlowerPickup : MonoBehaviour
{
    [Tooltip("How far away Luna can grab a free‑standing sprout.")]
    public float pickupRadius = 1f;

    private FlowerHolder _flowerHolder;

    void Awake()
    {
        _flowerHolder = GetComponent<FlowerHolder>();
    }

    void Update()
    {
        // Only try when pressing X and not already holding one
        if (Input.GetKeyDown(KeyCode.X) && !_flowerHolder.HasFlower)
        {
            // 1) Bail out if we’re standing over any garden,
            //    so FlowerInteractionManager’s garden logic wins:
            Collider2D[] allHits = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
            foreach (var col in allHits)
            {
                if (col.CompareTag("Garden"))
                {
                    // let the garden script handle this X
                    return;
                }
            }

            // 2) Otherwise, scan for loose sprouts:
            foreach (var col in allHits)
            {
                if (col.CompareTag("Sprout"))
                {
                    // convert to a normal Flower tag so it behaves the same downstream
                    col.gameObject.tag = "Flower";
                    // pick it up
                    _flowerHolder.PickUpFlower(col.gameObject);
                    return;
                }
            }
        }
    }

    // visualize the pickup radius
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
