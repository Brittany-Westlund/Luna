using UnityEngine;
using System.Collections;

public class LunariaWandAttractor : MonoBehaviour
{
    [Header("Attraction Settings")]
    public float    attractionRadius      = 5f;
    public float    attractionForce       = 2f;
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
    public KeyCode lightKey   = KeyCode.Q;
    public float    lightRadius = 1f;

    [Header("Cooldown Settings")]
    public float pickupCooldown = 2f;

    // internal state
    float   lastPickupTime     = -Mathf.Infinity;
    bool    isHeld            = false;
    bool    canPickup         = false;
    bool    hasLight          = false;
    GameObject activeVisualMote;
    GameObject luna;

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

        var hits = Physics2D.OverlapCircleAll(transform.position, attractionRadius, lightMoteLayer);
        foreach (var c in hits)
        {
            if (c.attachedRigidbody != null)
            {
                Vector2 dir = (transform.position - c.transform.position).normalized;
                c.attachedRigidbody.AddForce(dir * attractionForce);
            }
        }
    }

    void Update()
    {
        // Q: deliver light to sprouts or lanterns
        if (isHeld && hasLight && Input.GetKeyDown(lightKey))
            TryIlluminate();

        // V: pick up / drop wand
        if (Input.GetKeyDown(wandKey))
        {
            if (isHeld) DropWand();
            else        PickUpWand();
        }
    }

    private void TryIlluminate()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, lightRadius);
        foreach (var col in hits)
        {
            // 1) fully‑grown sprout?
            var spr = col.GetComponent<SproutAndLightManager>();
            if (spr != null)
            {
                spr.isPlayerNearby = true;
                spr.GiveLight();                        // grows & hides hint
                spr.isPlayerNearby = false;

                // enable LitFlowerB child
                var litB = col.transform.Find("LitFlowerB");
                if (litB != null && litB.TryGetComponent<SpriteRenderer>(out var fsr))
                    fsr.enabled = true;

                // remove any lingering light‑hint icon
                var hint = col.transform.Find("LightMoteIcon(Clone)");
                if (hint != null) Destroy(hint.gameObject);

                ConsumeMote();
                return;
            }

            // 2) any lantern tagged IndoorLantern or OutdoorLantern
            if (col.CompareTag("IndoorLantern") || col.CompareTag("OutdoorLantern"))
            {
                var litLantern = col.transform.Find("LitLantern");
                if (litLantern != null && litLantern.TryGetComponent<SpriteRenderer>(out var lsr) && !lsr.enabled)
                {
                    lsr.enabled = true;
                    ConsumeMote();
                    return;
                }
            }
        }
    }

    private void ConsumeMote()
    {
        hasLight = false;
        ResetWandVisuals();
        Debug.Log("🕯️ Light delivered!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // entering Luna’s trigger zone allows pick‑up
        if (!isHeld && other.CompareTag("Player"))
        {
            canPickup = true;
            return;
        }

        // while held, absorb motes on collision
        if (isHeld && !hasLight && other.CompareTag("LightMote") &&
            Time.time >= lastPickupTime + pickupCooldown)
        {
            hasLight       = true;
            lastPickupTime = Time.time;

            if (unlitFlower != null) unlitFlower.SetActive(false);
            if (litFlower   != null) litFlower  .SetActive(true);

            if (visualLightMotePrefab != null && lightMoteSpawnPoint != null)
            {
                activeVisualMote = Instantiate(
                    visualLightMotePrefab,
                    lightMoteSpawnPoint.position,
                    Quaternion.identity,
                    lightMoteSpawnPoint
                );
            }

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
        if (!canPickup) return;

        isHeld    = true;
        canPickup = false;

        // hide the pickup icon
        if (pickupIcon != null) pickupIcon.SetActive(false);

        // parent to Luna
        if (luna != null)
        {
            transform.SetParent(luna.transform);
            transform.localPosition = new Vector3(0.5f, 0.25f, 0f);
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

        // show the pickup icon again
        if (pickupIcon != null) pickupIcon.SetActive(true);

        transform.SetParent(null);
        if (luna != null)
        {
            transform.position = luna.transform.position + Vector3.up * 0.2f;
            transform.rotation = Quaternion.Euler(0, 0, 90);
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
        if (activeVisualMote != null)
        {
            Destroy(activeVisualMote);
            activeVisualMote = null;
        }
    }

    /// <summary>
    /// For external scripts (e.g. TeapotLightReceiver): does the wand currently hold a mote?
    /// </summary>
    public bool HasLight()
    {
        return hasLight;
    }

    /// <summary>
    /// For external scripts: consume the mote and reset wand visuals.
    /// Returns true if one was consumed.
    /// </summary>
    public bool GiveLightToObject()
    {
        if (!hasLight) 
            return false;

        hasLight = false;
        ResetWandVisuals();
        Debug.Log("✨ Light given via GiveLightToObject()");
        return true;
    }
}
