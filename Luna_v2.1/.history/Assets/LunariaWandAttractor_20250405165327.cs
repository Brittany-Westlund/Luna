using UnityEngine;

public class LunariaWandAttractor : MonoBehaviour
{
    [Header("Attraction Settings")]
    public float attractionRadius = 5f;
    public float attractionForce = 2f;
    public LayerMask lightMoteLayer;

    [Header("Wand Visuals")]
    public GameObject unlitFlower;
    public GameObject litFlower;

    private bool hasLight = false;

    void FixedUpdate()
    {
        // Find all colliders within the attraction radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attractionRadius, lightMoteLayer);

        foreach (Collider2D collider in colliders)
        {
            Rigidbody2D rb = collider.attachedRigidbody;

            if (rb != null)
            {
                Vector2 direction = (transform.position - rb.transform.position).normalized;
                rb.AddForce(direction * attractionForce);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("LightMote") && !hasLight)
        {
            AbsorbLight();
            Destroy(other.gameObject); // Remove the mote
        }
    }

    private void AbsorbLight()
    {
        hasLight = true;
        if (unlitFlower != null) unlitFlower.SetActive(false);
        if (litFlower != null) litFlower.SetActive(true);
    }

    public bool GiveLightToObject()
    {
        if (hasLight)
        {
            hasLight = false;
            if (unlitFlower != null) unlitFlower.SetActive(true);
            if (litFlower != null) litFlower.SetActive(false);
            return true;
        }
        return false;
    }

    public bool HasLight()
    {
        return hasLight;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
    }
}