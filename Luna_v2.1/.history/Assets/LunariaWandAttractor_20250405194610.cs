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
    public GameObject visualLightMotePrefab;
    public Transform lightMoteSpawnPoint;

    [Header("Cooldown Settings")]
    public float pickupCooldown = 2f;
    private float lastPickupTime = -Mathf.Infinity;

    [Header("Pickup & Drop Settings")]
    public KeyCode dropKey = KeyCode.G;
    public Transform dropPoint;
    private bool isHeld = true;

    private bool hasLight = false;
    private GameObject activeVisualLightMote;

    void FixedUpdate()
    {
        if (!isHeld) return;

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

    void Update()
    {
        if (isHeld && Input.GetKeyDown(dropKey))
        {
            DropWand();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isHeld) return;

        if (other.CompareTag("LightMote") && !hasLight && Time.time >= lastPickupTime + pickupCooldown)
        {
            AbsorbLight();
            Destroy(other.gameObject);
            lastPickupTime = Time.time;
        }
        else if (other.CompareTag("Teapot") && hasLight)
        {
            Debug.Log("Wand collided with teapot and will reset visuals.");
            hasLight = false;
            ResetWandVisuals();
        }
    }

    private void AbsorbLight()
    {
        hasLight = true;

        if (unlitFlower != null) unlitFlower.SetActive(false);
        if (litFlower != null) litFlower.SetActive(true);

        if (visualLightMotePrefab != null && lightMoteSpawnPoint != null)
        {
            activeVisualLightMote = Instantiate(visualLightMotePrefab, lightMoteSpawnPoint.position, Quaternion.identity, lightMoteSpawnPoint);
        }
    }

    public bool GiveLightToObject()
    {
        if (hasLight)
        {
            hasLight = false;
            ResetWandVisuals();
            return true;
        }
        return false;
    }

    public void ResetWandVisuals()
    {
        Debug.Log("ResetWandVisuals() called");

        if (litFlower != null)
        {
            litFlower.SetActive(false);
            litFlower.transform.SetParent(null);
            litFlower.transform.SetParent(transform);
            Debug.Log("✅ LitFlower disabled.");
        }

        if (unlitFlower != null)
        {
            unlitFlower.SetActive(true);
            unlitFlower.transform.SetParent(null);
            unlitFlower.transform.SetParent(transform);
            Debug.Log("✅ UnlitFlower enabled.");
        }

        if (activeVisualLightMote != null)
        {
            Destroy(activeVisualLightMote);
            Debug.Log("🧨 Destroyed visual mote.");
        }
    }

    public void DropWand()
    {
        Debug.Log("Luna dropped the wand.");
        isHeld = false;

        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        if (dropPoint != null)
        {
            transform.position = dropPoint.position;
        }
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
