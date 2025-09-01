using UnityEngine;
using System.Collections;

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
    public KeyCode pickupKey = KeyCode.E;
    public Vector3 dropOffset = new Vector3(1f, 0f, 0f);
    public Transform wandHoldPoint;

    [Header("Light Interaction")]
    [Tooltip("How far the wand can reach to illuminate a flower or lantern.")]
    public float lightRadius = 1f;
    [Tooltip("Key to press to release the light from the wand.")]
    public KeyCode lightKey = KeyCode.Q;

    private bool isHeld = false;
    private bool canPickup = false;
    private GameObject luna;
    private bool hasLight = false;
    private GameObject activeVisualLightMote;

    void Start()
    {
        luna = GameObject.FindWithTag("Player");

        // Ensure Rigidbody2D is configured properly
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.mass = 1f;
            rb.isKinematic = false;
        }

        // Ensure trigger is enabled
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

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
        // *** New: Q press to deliver light ***
        if (isHeld && hasLight && Input.GetKeyDown(lightKey))
        {
            TryIlluminate();
        }

        if (isHeld && Input.GetKeyDown(dropKey))
        {
            DropWand();
        }

        if (!isHeld && canPickup && Input.GetKeyDown(pickupKey))
        {
            PickUpWand();
        }
    }

    private void TryIlluminate()
    {
        // search around the wand
        var hits = Physics2D.OverlapCircleAll(transform.position, lightRadius);
        foreach (var col in hits)
        {
            // 1) fully‑grown sprout?
            var spr = col.GetComponent<SproutAndLightManager>();
            if (spr != null)
            {
                // temporarily mark “nearby” so GiveLight will fire
                spr.isPlayerNearby = true;
                spr.GiveLight();
                spr.isPlayerNearby = false;
                if (GiveLightToObject()) return;
            }

            // 2) lantern (tagged "Lantern", with a child named "LitLantern")
            if (col.CompareTag("Lantern"))
            {
                var lit = col.transform.Find("LitLantern");
                if (lit != null)
                {
                    var sr = lit.GetComponent<SpriteRenderer>();
                    if (sr != null && !sr.enabled)
                    {
                        sr.enabled = true;
                        if (GiveLightToObject()) return;
                    }
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isHeld)
        {
            if (other.CompareTag("Player"))
                canPickup = true;
            return;
        }

        // we no longer auto‑absorb or auto‑give on collision
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isHeld && other.CompareTag("Player"))
            canPickup = false;
    }

    private void AbsorbLight()
    {
        // this method is now unused—absorption happens via collision only
        hasLight = true;
        if (unlitFlower != null) unlitFlower.SetActive(false);
        if (litFlower   != null) litFlower  .SetActive(true);
        if (visualLightMotePrefab != null && lightMoteSpawnPoint != null)
            activeVisualLightMote = Instantiate(visualLightMotePrefab, lightMoteSpawnPoint.position, Quaternion.identity, lightMoteSpawnPoint);
    }

    public bool GiveLightToObject()
    {
        if (hasLight)
        {
            hasLight = false;
            ResetWandVisuals();
            Debug.Log("🕯️ Light delivered!");
            return true;
        }
        return false;
    }

    public void ResetWandVisuals()
    {
        if (litFlower != null) litFlower.SetActive(false);
        if (unlitFlower != null) unlitFlower.SetActive(true);
        if (activeVisualLightMote != null) Destroy(activeVisualLightMote);
    }

    private void PickUpWand()
    {
        isHeld = true;
        canPickup = false;
        if (wandHoldPoint != null)
        {
            transform.SetParent(wandHoldPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        }
        else if (luna != null)
        {
            transform.SetParent(luna.transform);
            transform.localPosition = new Vector3(0.5f, 0.25f, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        }
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = true;
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
        Debug.Log("🪄 Wand picked up.");
    }

    private void DropWand()
    {
        isHeld = false;
        canPickup = false;
        transform.SetParent(null);
        if (luna != null)
        {
            transform.position = luna.transform.position + dropOffset + Vector3.up * 0.2f;
            transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        }
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true;
        }
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
        Debug.Log("🔻 Wand dropped.");
    }

    public bool HasLight()
    {
        return hasLight;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attractionRadius);
        if (isHeld)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, lightRadius);
        }
    }
}


/* using UnityEngine;

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
    public KeyCode pickupKey = KeyCode.E;
    public Vector3 dropOffset = new Vector3(1f, 0f, 0f);
    public Transform wandHoldPoint;

    private bool isHeld = false;
    private bool canPickup = false;
    private GameObject luna;
    private bool hasLight = false;
    private GameObject activeVisualLightMote;

    void Start()
    {
        luna = GameObject.FindWithTag("Player");

        // Ensure Rigidbody2D is configured properly
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.mass = 1f; // Make sure mass is non-zero
            rb.isKinematic = false;
        }

        // Ensure trigger is enabled
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

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

        if (!isHeld && canPickup && Input.GetKeyDown(pickupKey))
        {
            PickUpWand();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isHeld)
        {
            if (other.CompareTag("Player"))
            {
                canPickup = true;
            }

            return;
        }

        // Wand is held and collided with something
        Debug.Log($"🔍 Triggered with: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("LightMote") && !hasLight && Time.time >= lastPickupTime + pickupCooldown)
        {
            AbsorbLight();
            Destroy(other.gameObject);
            lastPickupTime = Time.time;
        }
        else if (hasLight && other.CompareTag("Teapot"))
        {
            TeapotReceiver receiver = other.GetComponent<TeapotReceiver>();
            if (receiver != null)
            {
                receiver.ReceiveLight();
                GiveLightToObject();
                Debug.Log("✨ Gave light to teapot: " + other.name);
            }
        }
        else if (hasLight && other.transform.Find("LitFlowerB") != null)
        {
            Transform litFlower = other.transform.Find("LitFlowerB");
            Transform lightHint = other.transform.Find("LightMoteIcon(Clone)");

            if (lightHint != null && litFlower != null)
            {
                SpriteRenderer sr = litFlower.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.enabled = true;
                    Destroy(lightHint.gameObject);
                    GiveLightToObject();
                    Debug.Log("🌸 Lit up flower: " + other.name);
                }
            }
        }
        else if (hasLight && other.transform.Find("LitLantern") != null)
        {
            Transform litLantern = other.transform.Find("LitLantern");
            SpriteRenderer sr = litLantern?.GetComponent<SpriteRenderer>();
            if (sr != null && !sr.enabled)
            {
                sr.enabled = true;
                GiveLightToObject();
                Debug.Log("🕯️ Lit up lantern: " + other.name);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!isHeld && other.CompareTag("Player"))
        {
            canPickup = false;
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

        Debug.Log("💡 Light absorbed!");
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
        if (litFlower != null)
        {
            litFlower.SetActive(false);
            litFlower.transform.SetParent(transform);
        }

        if (unlitFlower != null)
        {
            unlitFlower.SetActive(true);
            unlitFlower.transform.SetParent(transform);
        }

        if (activeVisualLightMote != null)
        {
            Destroy(activeVisualLightMote);
        }

        Debug.Log("🔄 Wand visuals reset.");
    }

    private void PickUpWand()
    {
        isHeld = true;
        canPickup = false;

        if (wandHoldPoint != null)
        {
            transform.SetParent(wandHoldPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        }
        else if (luna != null)
        {
            transform.SetParent(luna.transform);
            transform.localPosition = new Vector3(0.5f, 0.25f, 0f);
            transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        }

        // Physics settings while held
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        Debug.Log("🪄 Wand picked up.");
    }

    private void DropWand()
    {
        isHeld = false;
        canPickup = false;

        transform.SetParent(null);

        if (luna != null)
        {
            transform.position = luna.transform.position + dropOffset + Vector3.up * 0.2f;
            transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        }

        // Keep Rigidbody dynamic but stable
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic = true; // stay kinematic to avoid being bumped
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        Debug.Log("🔻 Wand dropped.");
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
*/
