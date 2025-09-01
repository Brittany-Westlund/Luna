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

    [Header("Pickup & Drop Key")]
    [Tooltip("Press this key to pick up or drop the wand.")]
    public KeyCode wandKey = KeyCode.V;

    [Header("Pickup Icon (optional)")]
    [Tooltip("The icon GameObject to hide when the wand is picked up, and show when dropped.")]
    public GameObject pickupIcon;

    [Header("Light Delivery")]
    public KeyCode lightKey  = KeyCode.Q;
    public float    lightRadius = 1f;

    [Header("Cooldown Settings")]
    public float pickupCooldown = 2f;
    private float lastPickupTime = -Mathf.Infinity;

    private bool isHeld    = false;
    private bool canPickup = false;
    private bool hasLight  = false;
    private GameObject activeVisualLightMote;
    private GameObject luna;

    void Start()
    {
        luna = GameObject.FindWithTag("Player");
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) { rb.gravityScale = 0; rb.mass = 1f; rb.isKinematic = false; }
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void FixedUpdate()
    {
        if (!isHeld) return;
        var hits = Physics2D.OverlapCircleAll(transform.position, attractionRadius, lightMoteLayer);
        foreach (var c in hits)
            if (c.attachedRigidbody != null)
                c.attachedRigidbody.AddForce((Vector2)(transform.position - c.transform.position).normalized * attractionForce);
    }

    void Update()
    {
        // Q: deliver light
        if (isHeld && hasLight && Input.GetKeyDown(lightKey))
            TryIlluminate();

        // V: toggle wand pick‑up/drop and icon
        if (Input.GetKeyDown(wandKey))
        {
            if (isHeld) DropWand();
            else if (canPickup) PickUpWand();
        }
    }

    void TryIlluminate()
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
                    if (sr != null && !sr.enabled && ConsumeMote())
                        sr.enabled = true;
                }
            }
        }
    }

    bool ConsumeMote()
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
            hasLight = true;
            lastPickupTime = Time.time;
            if (unlitFlower != null) unlitFlower.SetActive(false);
            if (litFlower   != null) litFlower  .SetActive(true);
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
        isHeld    = true;
        canPickup = false;

        // hide icon
        if (pickupIcon != null) pickupIcon.SetActive(false);

        if (luna != null && wandKey == KeyCode.V)
        {
            transform.SetParent(luna.transform);
            transform.localPosition = new Vector3(0.5f, 0.25f, 0);
            transform.localRotation = Quaternion.Euler(0, 0, 45);
        }

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = true;
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        Debug.Log("🪄 Wand picked up.");
    }

    private void DropWand()
    {
        isHeld    = false;
        canPickup = false;

        // show icon
        if (pickupIcon != null) pickupIcon.SetActive(true);

        transform.SetParent(null);
        if (luna != null)
        {
            transform.position = luna.transform.position + Vector3.up * 0.2f;
            transform.rotation = Quaternion.Euler(0, 0, 90);
        }
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) { rb.velocity = Vector2.zero; rb.angularVelocity = 0; rb.isKinematic = true; }
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
}
