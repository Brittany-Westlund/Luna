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
        if (Input.GetKeyDown(KeyCode.X) && !_flowerHolder.HasFlower)
        {
            // scan for any Collider2D in range
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, pickupRadius);
            Debug.Log($"LooseFlowerPickup: saw {hits.Length} colliders in radius {pickupRadius}");
            foreach (var col in hits)
            {
                Debug.Log($"  → collider '{col.name}', tag='{col.tag}'");
                if (col.CompareTag("Sprout"))
                {
                    Debug.Log($"LooseFlowerPickup: picking up sprout '{col.name}'");
                    _flowerHolder.PickUpFlower(col.gameObject);
                    return;
                }
            }
        }
    }

    // draw the pickup radius in the Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRadius);
    }
}
