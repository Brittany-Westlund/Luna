using UnityEngine;
using System.Collections;

public class LunariaWandAttractor : MonoBehaviour
{
    [Header("Attraction Settings")]
    public float attractionRadius = 5f;
    public float attractionForce  = 2f;
    public LayerMask lightMoteLayer;

    [Header("Wand Visuals")]
    public GameObject unlitFlower;
    public GameObject litFlower;
    public GameObject visualLightMotePrefab;
    public Transform lightMoteSpawnPoint;

    [Header("Cooldown Settings")]
    public float pickupCooldown  = 2f;
    private float lastPickupTime = -Mathf.Infinity;

    [Header("Pickup & Drop Key")]
    [Tooltip("Press this key to pick up or drop the wand.")]
    public KeyCode wandKey = KeyCode.V;
    public Vector3 dropOffset = new Vector3(1f, 0f, 0f);
    public Transform wandHoldPoint;

    [Header("Light Delivery")]
    [Tooltip("Key to press when you want to give light to a flower or lantern.")]
    public KeyCode lightKey   = KeyCode.Q;
    [Tooltip("How far the wand can reach when you press Q.")]
    public float lightRadius  = 1f;

    private bool    isHeld    = false;
    private bool    canPickup = false;
    private GameObject luna;
    private bool    hasLight  = false;
    private GameObject activeVisualLightMote;

    void Start()
    {
        luna = GameObject.FindWithTag("Player");

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.mass         = 1f;
            rb.isKinematic  = false;
        }

        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void FixedUpdate()
    {
        if (!isHeld) return;

        var colliders = Physics2D.OverlapCircleAll(transform.position, attractionRadius, lightMoteLayer);
        foreach (var c in colliders)
        {
            var rb = c.attachedRigidbody;
            if (rb != null)
            {
                Vector2 dir = (transform.position - rb.transform.position).normalized;
                rb.AddForce(dir * attractionForce);
            }
        }
    }

    void Update()
    {
        // 1) Q‑press to deliver light if holding a mote
        if (isHeld && hasLight && Input.GetKeyDown(lightKey))
        {
            TryIlluminate();
        }

        // 2) V‑press toggles pickup/drop
        if (Input.GetKeyDown(wandKey))
        {
            if (isHeld)
                DropWand();
            else if (canPickup)
                PickUpWand();
        }
    }

    private void TryIlluminate()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, lightRadius);
        foreach (var col in hits)
        {
            var spr = col.GetComponent<SproutAndLightManager>();
            if (spr != null)
            {
                spr.isPlayerNearby = true;
                spr.GiveLight();
                spr.isPlayerNearby = false;
                if (ConsumeMote()) return;
            }

            if (col.CompareTag("Lantern"))
            {
                var lit = col.transform.Find("LitLantern");
                if (lit != null)
                {
                    var sr = lit.GetComponent<SpriteRenderer>();
                    if (sr != null && !sr.enabled)
                    {
                        sr.enabled = true;
                        if (ConsumeMote()) return;
                    }
                }
            }
        }
    }

    private bool ConsumeMote()
    {
        hasLight = false;
        ResetWandVisuals();
        Debug.Log("🕯️ Light delivered!");
        return true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isHeld && other.CompareTag("Player"))
        {
            canPickup = true;
            return;
        }

        if (isHeld && !hasLight && other.CompareTag("LightMote") &&
            Time.time >= lastPickupTime + pickupCooldown)
        {
            hasLight       = true;
            lastPickupTime = Time.time;

            if (unlitFlower != null)   unlitFlower.SetActive(false);
            if (litFlower   != null)   litFlower  .SetActive(true);

            if (visualLightMotePrefab != null && lightMoteSpawnPoint != null)
                activeVisualLightMote = Instantiate(
                    visualLightMotePrefab,
                    lightMoteSpawnPoint.position,
                    Quaternion.identity,
                    lightMoteSpawnPoint);

            Destroy(other.gameObject);
            Debug.Log("💡 Light absorbed!");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!isHeld && other.CompareTag("Player"))
            canPickup = false;
    }

    private void PickUpWand()
    {
        isHeld     = true;
        canPickup  = false;

        if (wandHoldPoint != null)
        {
            transform.SetParent(wandHoldPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(0,0,45);
        }
        else if (luna != null)
        {
            transform.SetParent(luna.transform);
            transform.localPosition = new Vector3(0.5f, 0.25f,0);
            transform.localRotation = Quaternion.Euler(0,0,45);
        }

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = true;
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        Debug.Log("💫 Wand picked up.");
    }

    private void DropWand()
    {
        isHeld    = false;
        canPickup = false;
        transform.SetParent(null);

        if (luna != null)
        {
            transform.position = luna.transform.position + dropOffset + Vector3.up * 0.2f;
            transform.rotation = Quaternion.Euler(0,0,90);
        }

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity        = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic     = true;
        }

        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        Debug.Log("🔻 Wand dropped.");
    }

    private void ResetWandVisuals()
    {
        if (litFlower != null)       litFlower.SetActive(false);
        if (unlitFlower != null)     unlitFlower.SetActive(true);
        if (activeVisualLightMote != null)
        {
            Destroy(activeVisualLightMote);
            activeVisualLightMote = null;
        }
    }

    public bool HasLight() => hasLight;

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
        if (isHeld && hasLight)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, lightRadius);
        }
    }
}
