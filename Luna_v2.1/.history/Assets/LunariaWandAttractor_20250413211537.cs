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
    public KeyCode pickupKey = KeyCode.E;
    public Vector3 dropOffset = new Vector3(1f, 0f, 0f);
    public Transform wandHoldPoint;

    private bool isHeld = true;
    private bool hasLight = false;
    private GameObject activeVisualLightMote;
    private Quaternion originalRotation;

    void Start()
    {
        originalRotation = transform.rotation;

        if (isHeld && wandHoldPoint != null)
        {
            HoldWand();
        }
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
        else if (!isHeld && Input.GetKeyDown(pickupKey))
        {
            TryPickupWand();
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

        transform.SetParent(null);

        GameObject luna = GameObject.FindWithTag("Player");
        if (luna != null)
        {
            transform.position = luna.transform.position + dropOffset;
            transform.rotation = Quaternion.Euler(0f, 0f, 90f);
        }

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }

    public void TryPickupWand()
    {
        GameObject luna = GameObject.FindWithTag("Player");
        if (luna != null && Vector3.Distance(transform.position, luna.transform.position) < 2f)
        {
            Debug.Log("Luna picked up the wand.");
            isHeld = true;
            HoldWand();
        }
    }

    private void HoldWand()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.isKinematic = true;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (wandHoldPoint == null)
        {
            GameObject luna = GameObject.FindWithTag("Player");
            if (luna != null) wandHoldPoint = luna.transform;
        }

        transform.SetParent(wandHoldPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
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
