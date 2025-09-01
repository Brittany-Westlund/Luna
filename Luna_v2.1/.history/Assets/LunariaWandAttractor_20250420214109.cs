using UnityEngine;
using System.Collections;

public class LunariaWandAttractor : MonoBehaviour
{
    [Header("Attraction Settings")]
    public float    attractionRadius = 5f;
    public float    attractionForce  = 2f;
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
    [Tooltip("Icon to hide when the wand is picked up, and show when dropped.")]
    public GameObject pickupIcon;

    [Header("Light Delivery")]
    public KeyCode lightKey   = KeyCode.Q;
    public float    lightRadius = 0.2f;

    [Header("Cooldown Settings")]
    public float pickupCooldown = 2f;

    // internal state
    float   _lastPickupTime = -Mathf.Infinity;
    bool    _isHeld         = false;
    bool    _canPickup      = false;
    bool    _hasLight       = false;
    GameObject _activeMote;
    Transform  _luna;

    void Start()
    {
        _luna = GameObject.FindWithTag("Player")?.transform;

        // ensure physics setup
        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.gravityScale = 0;
            rb.mass         = 1f;
            rb.isKinematic  = false;
        }
        if (TryGetComponent<Collider2D>(out var col))
            col.isTrigger = true;
    }

    void FixedUpdate()
    {
        if (!_isHeld) return;

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
        // Q: deliver light
        if (_isHeld && _hasLight && Input.GetKeyDown(lightKey))
            TryIlluminate();

        // V: toggle pickup/drop
        if (Input.GetKeyDown(wandKey))
        {
            if (_isHeld) DropWand();
            else         PickUpWand();
        }
    }

    private void TryIlluminate()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, lightRadius);
        foreach (var col in hits)
        {
            // only light fully‑grown sprouts
            if (col.TryGetComponent<SproutAndLightManager>(out var spr) 
            && spr.IsFullyGrown)
            {
            spr.isPlayerNearby = true;
            spr.GiveLight();
            spr.isPlayerNearby = false;

                // enable LitFlowerB
                var litB = col.transform.Find("LitFlowerB");
                if (litB?.TryGetComponent<SpriteRenderer>(out var fsr) == true)
                    fsr.enabled = true;

                // destroy hint icon
                var hint = col.transform.Find("LightMoteIcon(Clone)");
                if (hint != null) Destroy(hint.gameObject);

                ConsumeMote();
                return;
            }

            // lantern?
            if (col.CompareTag("IndoorLantern") || col.CompareTag("OutdoorLantern"))
            {
                var litLantern = col.transform.Find("LitLantern");
                if (litLantern?.TryGetComponent<SpriteRenderer>(out var lsr) == true && !lsr.enabled)
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
        _hasLight = false;
        ResetWandVisuals();
        Debug.Log("🕯️ Light delivered!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // allow pickup only when Luna enters range
        if (!_isHeld && other.CompareTag("Player"))
        {
            _canPickup = true;
            Debug.Log("WAND: canPickup = true");
            return;
        }

        // absorb motes
        if (_isHeld && !_hasLight && other.CompareTag("LightMote") &&
            Time.time >= _lastPickupTime + pickupCooldown)
        {
            _hasLight       = true;
            _lastPickupTime = Time.time;

            if (unlitFlower != null) unlitFlower.SetActive(false);
            if (litFlower   != null) litFlower  .SetActive(true);

            if (visualLightMotePrefab != null && lightMoteSpawnPoint != null)
                _activeMote = Instantiate(
                    visualLightMotePrefab,
                    lightMoteSpawnPoint.position,
                    Quaternion.identity,
                    lightMoteSpawnPoint
                );

            Destroy(other.gameObject);
            Debug.Log("💡 Light absorbed!");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!_isHeld && other.CompareTag("Player"))
        {
            _canPickup = false;
            Debug.Log("WAND: canPickup = false");
        }
    }

    private void PickUpWand()
    {
        if (!_canPickup) return;

        _isHeld    = true;
        _canPickup = false;

        if (pickupIcon != null) pickupIcon.SetActive(false);

        if (_luna != null)
        {
            transform.SetParent(_luna);
            transform.localPosition = new Vector3(0.5f, 0.25f, 0f);
            transform.localRotation = Quaternion.Euler(0,0,45);
        }

        if (TryGetComponent<Rigidbody2D>(out var rb)) rb.isKinematic = true;
        if (TryGetComponent<Collider2D>(out var c))    c.isTrigger    = true;

        Debug.Log("🪄 Wand picked up.");
    }

    private void DropWand()
    {
        _isHeld    = false;
        _canPickup = false;

        if (pickupIcon != null)
            pickupIcon.SetActive(true);

        // un‑parent so it falls freely
        transform.SetParent(null);

        // snap to just above Luna
        if (_luna != null)
            transform.position = _luna.position + Vector3.up * 0.2f;
        
        // rotate back 45° around Z
        transform.rotation = Quaternion.Euler(0f, 0f, 90f);

        // clear any residual motion
        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.velocity        = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.isKinematic     = true;
        }

        if (TryGetComponent<Collider2D>(out var c))
            c.isTrigger = true;

        Debug.Log("🔻 Wand dropped vertically.");
    }

    private void ResetWandVisuals()
    {
        if (litFlower != null)       litFlower.SetActive(false);
        if (unlitFlower != null)     unlitFlower.SetActive(true);
        if (_activeMote != null)
        {
            Destroy(_activeMote);
            _activeMote = null;
        }
    }

    /// <summary>
    /// For external scripts (TeapotLightReceiver) — does the wand hold a mote?
    /// </summary>
    public bool HasLight() => _hasLight;

    /// <summary>
    /// For external scripts — consume the mote & reset visuals.
    /// Returns true if one was consumed.
    /// </summary>
    public bool GiveLightToObject()
    {
        if (!_hasLight) return false;
        _hasLight = false;
        ResetWandVisuals();
        Debug.Log("✨ Light given via GiveLightToObject()");
        return true;
    }
}
