using UnityEngine;
using System.Linq;
using System.Collections;

public class LunariaWandAttractor : MonoBehaviour
{
    [Header("Hold Points")]
    public Transform groundHoldPoint;
    public Transform flightHoldPoint;

    [Header("Pickup & Drop")]
    public KeyCode wandKey = KeyCode.V;
    public GameObject pickupIcon;

    [Header("Light Delivery")]
    public KeyCode lightKey = KeyCode.Q;
    public float lightRadius = 1f;
    public float pickupCooldown = 2f;

    [Header("Mote Attraction")]
    public float attractionRadius = 2f;
    public float attractionForce = 2f;
    public LayerMask lightMoteLayer;

    [Header("Mote Visuals")]
    public GameObject visualLightMotePrefab;
    public Transform lightMoteSpawnPoint;

    [Header("Wand Visuals")]
    public GameObject unlitFlower;
    public GameObject litFlower;

    [Header("Pickup Rotation")]
    [Tooltip("Z‑angle (in degrees) to apply when Luna picks up the wand")]
    public float pickUpRotationAngle = 75f;

    // internal state
    Transform           _luna;
    ButterflyFlyHandler _fly;
    bool                _isHeld         = false;
    bool                _canPickup      = false;
    bool                _hasLight       = false;
    float               _lastPickupTime = -Mathf.Infinity;
    GameObject          _activeMote;
    Vector3             _initialWorldScale;
    Transform           _currentHoldPoint;
    
    [Header("Attracted Mote Audio")]
    public AudioSource audioSource;

    [Header("Pickup/Putdown Audio")]
    public AudioSource pickupSFX;
    public AudioSource putdownSFX;

    void Awake()
    {
        // record the wand’s world‑scale before any parenting
        _initialWorldScale = transform.lossyScale;
    }

    void Start()
    {
        _luna = GameObject.FindWithTag("Player")?.transform;
        _fly  = FindObjectOfType<ButterflyFlyHandler>();

        // ensure physics setup
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) { rb.gravityScale = 0; rb.isKinematic = false; }
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        // find or add an AudioSource for SFX!
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void FixedUpdate()
    {
        if (!_isHeld) return;

        // pull in light motes
        var hits = Physics2D.OverlapCircleAll(transform.position, attractionRadius, lightMoteLayer);
        foreach (var c in hits)
            if (c.attachedRigidbody != null)
            {
                Vector2 dir = (transform.position - c.transform.position).normalized;
                c.attachedRigidbody.AddForce(dir * attractionForce);
            }
    }

    void Update()
    {
        // Q: deliver light
        if (_isHeld && _hasLight && Input.GetKeyDown(lightKey))
            TryIlluminate();

        // V: pick up / drop
        if (Input.GetKeyDown(wandKey))
        {
            if (_isHeld) DropWand();
            else         PickUpWand();
        }

        // auto‑snap (in case fly state changed mid‑hold)
        if (_isHeld && _fly != null)
        {
            Transform desired = _fly._isFlying ? flightHoldPoint : groundHoldPoint;
            if (transform.parent != desired)
                SnapUnder(desired);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (_isHeld)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, lightRadius);
        }
    }

    private void TryIlluminate()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, lightRadius);

        var candidate = hits
            .Where(c =>
                (c.TryGetComponent<SproutAndLightManager>(out var s)
                     && s.IsFullyGrown
                     && !s.litFlowerRenderer.enabled)
             || c.GetComponentInParent<TeapotLightReceiver>() != null
             || ((c.CompareTag("IndoorLantern")||c.CompareTag("OutdoorLantern"))
                 && c.transform.Find("LitLantern")?.GetComponent<SpriteRenderer>() is SpriteRenderer lsr
                 && !lsr.enabled)
            )
            .OrderBy(c => Vector2.Distance(transform.position, c.transform.position))
            .FirstOrDefault();

        if (candidate == null) return;

        // — Flower —
        if (candidate.TryGetComponent<SproutAndLightManager>(out var flower))
        {
            flower.isPlayerNearby = true;
            flower.GiveLight();
            flower.isPlayerNearby = false;

            // destroy its hint icon if present
            var hint = candidate.transform.Find("LightMoteIcon(Clone)");
            if (hint != null) Destroy(hint.gameObject);

            var litB = candidate.transform.Find("LitFlowerB")?.GetComponent<SpriteRenderer>();
            if (litB != null) litB.enabled = true;

            ConsumeMote();
            return;
        }

        // — Teapot —
        var receiver = candidate.GetComponentInParent<TeapotLightReceiver>();
        if (receiver != null)
        {
            if (GiveLightToObject())
                receiver.ActivateBrewReadyState();
            return;
        }

        // — Lantern —
        var lanternSR = candidate.transform.Find("LitLantern")?.GetComponent<SpriteRenderer>();
        if (lanternSR != null && !lanternSR.enabled)
        {
            lanternSR.enabled = true;
            ConsumeMote();
        }
    }

    private bool ConsumeMote()
    {
        if (!_hasLight) return false;
        _hasLight = false;
        ResetWandVisuals();
        return true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isHeld && other.CompareTag("Player"))
        {
            _canPickup = true;
            return;
        }

        // absorb light motes
        if (_isHeld
            && !_hasLight
            && other.CompareTag("LightMote")
            && Time.time >= _lastPickupTime + pickupCooldown)
        {
            _hasLight       = true;
            _lastPickupTime = Time.time;

            unlitFlower?.SetActive(false);
            litFlower  ?.SetActive(true);

            StartCoroutine(PopScale(litFlower.transform));

            if (visualLightMotePrefab != null && lightMoteSpawnPoint != null)
            {
                _activeMote = Instantiate(
                    visualLightMotePrefab,
                    lightMoteSpawnPoint.position,
                    Quaternion.identity,
                    lightMoteSpawnPoint);

                // disable physics so it won't collide under the butterfly
                var moteRb = _activeMote.GetComponent<Rigidbody2D>();
                if (moteRb != null) moteRb.isKinematic = true;
                var moteCol = _activeMote.GetComponent<Collider2D>();
                if (moteCol != null) moteCol.enabled = false;
            }

            // ---- PLAY THE SOUND HERE ----
            if (audioSource != null && audioSource.clip != null)
                audioSource.Play();
           
            Destroy(other.gameObject);
        }
    }

    IEnumerator PopScale(Transform obj, float popScale = 1.3f, float duration = 0.15f)
    {
        if (obj == null) yield break;
        Vector3 originalScale = obj.localScale;
        Vector3 targetScale = originalScale * popScale;
        float elapsed = 0f;

        // Scale up
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            obj.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }
        obj.localScale = targetScale;

        // Scale back
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            obj.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }
        obj.localScale = originalScale;
    }


    void OnTriggerExit2D(Collider2D other)
    {
        if (!_isHeld && other.CompareTag("Player"))
            _canPickup = false;
    }

    private void PickUpWand()
    {
        if (!_canPickup) return;

        _isHeld = true;
        pickupIcon?.SetActive(false);

        // PLAY PICKUP SOUND
        if (pickupSFX) pickupSFX.Play();

        bool flying = (_fly != null && _fly._isFlying);
        _currentHoldPoint = flying ? flightHoldPoint : groundHoldPoint;

        SnapUnder(_currentHoldPoint);
    }

    private void DropWand()
    {
        _isHeld = false;
        pickupIcon?.SetActive(true);

        // PLAY PUTDOWN SOUND
        if (putdownSFX) putdownSFX.Play();

        // unparent and preserve world transform
        transform.SetParent(null, true);
        if (_luna != null)
            transform.position = _luna.position + Vector3.up * 0.2f;

        // ensure the wand returns upright
        transform.rotation = Quaternion.Euler(0f, 0f, 90f);
    }

    private void ResetWandVisuals()
    {
        litFlower?.SetActive(false);
        unlitFlower?.SetActive(true);
        if (_activeMote != null) { Destroy(_activeMote); _activeMote = null; }
    }

    /// <summary>
    /// Parent under `target` while preserving world‑position & scale,
    /// then explicitly set a world rotation.
    /// </summary>
    private void SnapUnder(Transform target)
    {
        // parent, keep world pos/rot/scale
        transform.SetParent(target, true);
        transform.localPosition = Vector3.zero;

        // override world rotation so socket's rotation doesn't flip us
        transform.rotation = Quaternion.Euler(0f, 0f, pickUpRotationAngle);

        // restore original world scale despite parent scaling
        Vector3 ps = target.lossyScale;
        transform.localScale = new Vector3(
            _initialWorldScale.x / ps.x,
            _initialWorldScale.y / ps.y,
            _initialWorldScale.z / ps.z
        );
    }

    /// <summary> Does the wand currently hold a mote? </summary>
    public bool HasLight() => _hasLight;

    /// <summary>
    /// For external scripts: consume the mote & reset visuals. Returns true if consumed.
    /// </summary>
    public bool GiveLightToObject()
    {
        if (!_hasLight) return false;
        _hasLight = false;
        ResetWandVisuals();
        return true;
    }
}



/* using UnityEngine;
using System.Linq;

public class LunariaWandAttractor : MonoBehaviour
{
    [Header("Hold Points")]
    public Transform groundHoldPoint;
    public Transform flightHoldPoint;

    [Header("Pickup & Drop")]
    public KeyCode wandKey = KeyCode.V;
    public GameObject pickupIcon;

    [Header("Light Delivery")]
    public KeyCode lightKey = KeyCode.Q;
    public float lightRadius = 1f;
    public float pickupCooldown = 2f;

    [Header("Mote Attraction")]
    public float attractionRadius = 2f;
    public float attractionForce = 2f;
    public LayerMask lightMoteLayer;

    [Header("Mote Visuals")]
    public GameObject visualLightMotePrefab;
    public Transform lightMoteSpawnPoint;

    [Header("Wand Visuals")]
    public GameObject unlitFlower;
    public GameObject litFlower;

    [Header("Pickup Rotation")]
    [Tooltip("Z‑angle (in degrees) to apply when Luna picks up the wand")]
    public float pickUpRotationAngle = 75f;

    // internal state
    Transform      _luna;
    ButterflyFlyHandler _fly;
    bool           _isHeld   = false;
    bool           _canPickup= false;
    bool           _hasLight = false;
    float          _lastPickupTime = -Mathf.Infinity;
    GameObject     _activeMote;
    Vector3        _initialWorldScale;
    Transform      _currentHoldPoint;

    void Awake()
    {
        // record the wand’s world‐scale before any parenting
        _initialWorldScale = transform.lossyScale;
    }

    void Start()
    {
        _luna = GameObject.FindWithTag("Player")?.transform;
        _fly  = FindObjectOfType<ButterflyFlyHandler>();

        // ensure physics setup
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) { rb.gravityScale = 0; rb.isKinematic = false; }
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void FixedUpdate()
    {
        if (!_isHeld) return;

        // pull in light motes
        var hits = Physics2D.OverlapCircleAll(transform.position, attractionRadius, lightMoteLayer);
        foreach (var c in hits)
            if (c.attachedRigidbody != null)
            {
                Vector2 dir = (transform.position - c.transform.position).normalized;
                c.attachedRigidbody.AddForce(dir * attractionForce);
            }
    }

    void Update()
    {
        // Q: deliver light
        if (_isHeld && _hasLight && Input.GetKeyDown(lightKey))
            TryIlluminate();

        // V: pick up / drop
        if (Input.GetKeyDown(wandKey))
        {
            if (_isHeld) DropWand();
            else         PickUpWand();
        }

        // auto‑snap (in case fly state changed mid‑hold)
        if (_isHeld && _fly != null)
        {
            Transform desired = _fly._isFlying ? flightHoldPoint : groundHoldPoint;
            if (transform.parent != desired)
                SnapUnder(desired);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (_isHeld)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, lightRadius);
        }
    }

    private void TryIlluminate()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, lightRadius);

        var candidate = hits
            .Where(c =>
                (c.TryGetComponent<SproutAndLightManager>(out var s)
                     && s.IsFullyGrown
                     && !s.litFlowerRenderer.enabled)
             || c.GetComponentInParent<TeapotLightReceiver>() != null
             || ((c.CompareTag("IndoorLantern")||c.CompareTag("OutdoorLantern"))
                 && c.transform.Find("LitLantern")?.GetComponent<SpriteRenderer>() is SpriteRenderer lsr
                 && !lsr.enabled)
            )
            .OrderBy(c => Vector2.Distance(transform.position, c.transform.position))
            .FirstOrDefault();

        if (candidate == null) return;

        // — Flower —
        if (candidate.TryGetComponent<SproutAndLightManager>(out var flower))
        {
            flower.isPlayerNearby = true;
            flower.GiveLight();
            flower.isPlayerNearby = false;

            // destroy its hint icon if present
            var hint = candidate.transform.Find("LightMoteIcon(Clone)");
            if (hint != null) Destroy(hint.gameObject);

            var litB = candidate.transform.Find("LitFlowerB")?.GetComponent<SpriteRenderer>();
            if (litB != null) litB.enabled = true;

            ConsumeMote();
            return;
        }

        // — Teapot —
        var receiver = candidate.GetComponentInParent<TeapotLightReceiver>();
        if (receiver != null)
        {
            if (GiveLightToObject())
                receiver.ActivateBrewReadyState();
            return;
        }

        // — Lantern —
        var lanternSR = candidate.transform.Find("LitLantern")?.GetComponent<SpriteRenderer>();
        if (lanternSR != null && !lanternSR.enabled)
        {
            lanternSR.enabled = true;
            ConsumeMote();
        }
    }

    private bool ConsumeMote()
    {
        if (!_hasLight) return false;
        _hasLight = false;
        ResetWandVisuals();
        return true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isHeld && other.CompareTag("Player"))
        {
            _canPickup = true;
            return;
        }

        // absorb light motes
        if (_isHeld
            && !_hasLight
            && other.CompareTag("LightMote")
            && Time.time >= _lastPickupTime + pickupCooldown)
        {
            _hasLight       = true;
            _lastPickupTime = Time.time;

            unlitFlower?.SetActive(false);
            litFlower  ?.SetActive(true);

            if (visualLightMotePrefab != null && lightMoteSpawnPoint != null)
                _activeMote = Instantiate(
                    visualLightMotePrefab,
                    lightMoteSpawnPoint.position,
                    Quaternion.identity,
                    lightMoteSpawnPoint);

            Destroy(other.gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!_isHeld && other.CompareTag("Player"))
            _canPickup = false;
    }

    private void PickUpWand()
    {
        if (!_canPickup) return;

        _isHeld = true;
        pickupIcon?.SetActive(false);

        // choose the correct socket
        bool flying = (_fly != null && _fly._isFlying);
        _currentHoldPoint = flying ? flightHoldPoint : groundHoldPoint;

        SnapUnder(_currentHoldPoint);
    }

    private void DropWand()
    {
        _isHeld = false;
        pickupIcon?.SetActive(true);

        // unparent and preserve world transform
        transform.SetParent(null, true);
        if (_luna != null)
            transform.position = _luna.position + Vector3.up * 0.2f;

        // Unity will preserve world scale on unparent with worldPositionStays=true
        transform.rotation   = Quaternion.Euler(0f, 0f, 90f);
    }

    private void ResetWandVisuals()
    {
        litFlower?.SetActive(false);
        unlitFlower?.SetActive(true);
        if (_activeMote != null) { Destroy(_activeMote); _activeMote = null; }
    }

    /// <summary>
    /// Parent under `target` while preserving world‐position, rotation, & scale.
    /// </summary>
    private void SnapUnder(Transform target)
    {
        transform.SetParent(target, true);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0f, 0f, pickUpRotationAngle);

        // enforce your original world scale:
        Vector3 ps = target.lossyScale;
        transform.localScale = new Vector3(
            _initialWorldScale.x / ps.x,
            _initialWorldScale.y / ps.y,
            _initialWorldScale.z / ps.z
        );
    }

    /// <summary> Does the wand currently hold a mote? </summary>
    public bool HasLight() => _hasLight;

    /// <summary>
    /// For external scripts: consume the mote & reset visuals. Returns true if consumed.
    /// </summary>
    public bool GiveLightToObject()
    {
        if (!_hasLight) return false;
        _hasLight = false;
        ResetWandVisuals();
        return true;
    }
}
*/