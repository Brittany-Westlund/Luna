using UnityEngine;
using System.Collections;

public class LunariaWandAttractor : MonoBehaviour
{
    [Header("Hold Points")]
    public Transform groundHoldPoint; // drag your Luna hand here
    public Transform flightHoldPoint; // drag your butterfly socket here
    private Transform _currentHoldPoint;

    [Header("Pickup & Drop")]
    public KeyCode wandKey = KeyCode.V;
    public GameObject pickupIcon;

    [Header("Light Delivery")]
    public KeyCode lightKey = KeyCode.Q;
    public float lightRadius = 0.1f;
    public float pickupCooldown = 2f;

    [Header("Mote Attraction")]
    public float attractionRadius = 2f;
    public float attractionForce = 2f;
    public LayerMask lightMoteLayer;
    public GameObject visualLightMotePrefab;
    public Transform lightMoteSpawnPoint;
    public GameObject unlitFlower, litFlower;

    [Header("Pickup Rotation")]
    [Tooltip("Z‑angle (in degrees) to apply when Luna picks up the wand")]
    public float pickUpRotationAngle = 75f;

    Transform _luna;
    ButterflyFlyHandler _fly;
    float    _lastPickupTime = -Mathf.Infinity;
    bool     _isHeld         = false;
    bool     _canPickup      = false;
    bool     _hasLight       = false;
    GameObject _activeMote;

    Vector3 _initialWorldScale;

    void Awake()
    {
        // capture _world_ scale so we never shrink
        _initialWorldScale = transform.lossyScale;
    }

    void Start()
    {
        _luna = GameObject.FindWithTag("Player")?.transform;
        _fly  = FindObjectOfType<ButterflyFlyHandler>();

        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) { rb.gravityScale = 0; rb.isKinematic = false; }
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void FixedUpdate()
    {
        if (!_isHeld) return;

        // attract motes
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

        // V: pickup / drop
        if (Input.GetKeyDown(wandKey))
        {
            if (_isHeld) DropWand();
            else         PickUpWand();
        }

        // **auto‐snap into the correct socket** if held
        if (_isHeld && _fly != null)
        {
            Transform desired = _fly._isFlying ? flightHoldPoint : groundHoldPoint;
            if (transform.parent != desired)
            {
                // world‐stays = true preserves position/scale/rotation
                transform.SetParent(desired, true);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.Euler(0f, 0f, pickUpRotationAngle);

                // re‐enforce world scale
                transform.localScale = new Vector3(
                  _initialWorldScale.x / desired.lossyScale.x,
                  _initialWorldScale.y / desired.lossyScale.y,
                  _initialWorldScale.z / desired.lossyScale.z
                );
            }
        }
    }

    private void TryIlluminate()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, lightRadius);
        if (hits.Length == 0) return;

        foreach (var col in hits)
        {
            if (col.TryGetComponent<TeapotLightReceiver>(out var teapot))
            {
                // give it the mote
                if (ConsumeMote())  // make ConsumeMote return bool (moved below)
                {
                    teapot.ActivateBrewReadyState();
                    return;
                }
            }
            
            // 1) Sprout?
            if (col.TryGetComponent<SproutAndLightManager>(out var spr) 
                && spr.IsFullyGrown 
                && !spr.litFlowerRenderer.enabled)
            {
                spr.isPlayerNearby = true;
                spr.GiveLight();
                spr.isPlayerNearby = false;

                var litB = col.transform.Find("LitFlowerB")
                            ?.GetComponent<SpriteRenderer>();
                if (litB != null) litB.enabled = true;

                var hint = col.transform.Find("LightMoteIcon(Clone)");
                if (hint != null) Destroy(hint.gameObject);

                ConsumeMote();
                return;
            }

            // 2) Teapot?
            if (col.TryGetComponent<TeapotLightReceiver>(out var teapotReceiver))
            {
                if (ConsumeMote())      // now returns bool
                {
                    teapot.ActivateBrewReadyState();
                    return;
                }
            }

            // 3) Lantern?
            if ((col.CompareTag("IndoorLantern") || col.CompareTag("OutdoorLantern"))
                && col.transform.Find("LitLantern")
                    ?.TryGetComponent<SpriteRenderer>(out var lsr) == true
                && !lsr.enabled)
            {
                lsr.enabled = true;
                ConsumeMote();
                return;
            }
        }
    }

    private bool ConsumeMote()
    {
        if (!_hasLight) return false;
        _hasLight = false;
        ResetWandVisuals();
        Debug.Log("🕯️ Light delivered!");
        return true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isHeld && other.CompareTag("Player"))
        {
            _canPickup = true;
        }
        else if (_isHeld && !_hasLight && other.CompareTag("LightMote")
             && Time.time >= _lastPickupTime + pickupCooldown)
        {
            // absorb
            _hasLight = true;
            _lastPickupTime = Time.time;
            if (unlitFlower!=null) unlitFlower.SetActive(false);
            if (litFlower  !=null) litFlower  .SetActive(true);
            if (visualLightMotePrefab!=null && lightMoteSpawnPoint!=null)
                _activeMote = Instantiate(visualLightMotePrefab,
                                          lightMoteSpawnPoint.position,
                                          Quaternion.identity,
                                          lightMoteSpawnPoint);
            Destroy(other.gameObject);
            Debug.Log("💡 Light absorbed!");
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
        if (pickupIcon != null) 
            pickupIcon.SetActive(false);

        // disable physics while held
        if (TryGetComponent<Rigidbody2D>(out var rb)) 
            rb.isKinematic = true;
        if (TryGetComponent<Collider2D>(out var c))    
            c.isTrigger = true;

        // parent immediately under your chosen hold point:
        // (assuming you've already set `_currentHoldPoint` in Update or Awake)
        transform.SetParent(_currentHoldPoint, true);

        // snap into place
        transform.localPosition = Vector3.zero;

        // (A) force world‑space
        transform.rotation = Quaternion.Euler(0f, 0f, 75);


        // restore whatever scale you want…
        // transform.localScale = _initialLocalScale; 

        Debug.Log($"🪄 Wand picked up and rotated to {pickUpRotationAngle}°.");
    }

    private void DropWand()
    {
        _isHeld    = false;
        _canPickup = false;
        if (pickupIcon!=null) pickupIcon.SetActive(true);

        // un‐parent and drop above Luna
        transform.SetParent(null, true);
        if (_luna!=null) transform.position = _luna.position + Vector3.up*0.2f;
        transform.rotation = Quaternion.Euler(0,0,90);
        transform.localScale = _initialWorldScale;

        var rb = GetComponent<Rigidbody2D>();
        if (rb!=null) { rb.velocity=Vector2.zero; rb.angularVelocity=0; rb.isKinematic=true; }
        var c  = GetComponent<Collider2D>(); if (c!=null) c.isTrigger=true;

        Debug.Log("🔻 Wand dropped.");
    }

    private void ResetWandVisuals()
    {
        if (litFlower   != null) litFlower  .SetActive(false);
        if (unlitFlower != null) unlitFlower.SetActive(true);
        if (_activeMote != null) { Destroy(_activeMote); _activeMote = null; }
    }

    public bool HasLight() => _hasLight;

    /// <summary>
    /// For external scripts (like TeapotLightReceiver) — consume the mote & reset visuals.
    /// Returns true if one was consumed.
    /// </summary>
    public bool GiveLightToObject()
    {
        if (!_hasLight) return false;
        _hasLight = false;
        ResetWandVisuals();
        Debug.Log("✨ Light delivered via GiveLightToObject()");
        return true;
    }

}


/* using UnityEngine;
using System.Collections;
using System.Linq;

public class LunariaWandAttractor : MonoBehaviour
{
    [Header("Attraction Settings")]
    public float    attractionRadius = 2f;
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
    public float    lightRadius = 0.1f;

    [Header("Cooldown Settings")]
    public float pickupCooldown = 2f;

     [Header("Hold Points")]
    public Transform groundHoldPoint;    // assign your Luna’s Transform
    public Transform flightHoldPoint;

    // internal state
    float   _lastPickupTime = -Mathf.Infinity;
    bool    _isHeld         = false;
    bool    _canPickup      = false;
    bool    _hasLight       = false;
    GameObject _activeMote;
    Transform  _luna;

    Vector3 _initialLocalScale;
    Transform _currentHoldPoint;
   void Awake()
    {
        // remember what scale the wand starts with
        _initialLocalScale = transform.localScale;
    }
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
    // grab everything in radius
    var hits = Physics2D.OverlapCircleAll(transform.position, lightRadius);
    if (hits.Length == 0) return;

    // pick the closest *unlit* fully‑grown sprout or *unlit* lantern
    var candidate = hits
        .Where(c =>
        {
            // 1) unlit, fully‑grown sprout?
            if (c.TryGetComponent<SproutAndLightManager>(out var spr) 
                && spr.IsFullyGrown 
                && !spr.litFlowerRenderer.enabled)
            {
                return true;
            }
            // 2) unlit lantern?
            if (c.CompareTag("IndoorLantern") || c.CompareTag("OutdoorLantern"))
            {
                var lit = c.transform.Find("LitLantern")?.GetComponent<SpriteRenderer>();
                if (lit != null && !lit.enabled) return true;
            }
            return false;
        })
        .OrderBy(c => Vector2.Distance(transform.position, c.transform.position))
        .FirstOrDefault();

    if (candidate == null) return;

    // if it's a flower…
    if (candidate.TryGetComponent<SproutAndLightManager>(out var flower))
    {
        flower.isPlayerNearby = true;
        flower.GiveLight();
        flower.isPlayerNearby = false;

        // turn on its LitFlowerB sprite
        var litB = candidate.transform.Find("LitFlowerB")?.GetComponent<SpriteRenderer>();
        if (litB != null) litB.enabled = true;

        // destroy its hint icon
        var hint = candidate.transform.Find("LightMoteIcon(Clone)");
        if (hint != null) Destroy(hint.gameObject);

        ConsumeMote();
        return;
    }

    // …otherwise it's a lantern
    {
        var litLantern = candidate.transform.Find("LitLantern")?.GetComponent<SpriteRenderer>();
        if (litLantern != null && !litLantern.enabled)
        {
            litLantern.enabled = true;
            ConsumeMote();
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

       // choose which socket to attach to
        // assume you have some way to know whether you're flying:
        bool isFlying = FindObjectOfType<ButterflyFlyHandler>()._isFlying;
        _currentHoldPoint = isFlying ? flightHoldPoint : groundHoldPoint;

        // parent under the chosen hold point, preserve world pos/rot/scale
        transform.SetParent(_currentHoldPoint, true);

        // snap into exactly the hold‐point
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // *restore* its original scale so it never “shrinks”
        transform.localScale = _initialLocalScale;

        // hide pickup icon, disable physics, etc…
        if (pickupIcon != null) pickupIcon.SetActive(false);
        if (TryGetComponent<Rigidbody2D>(out var rb)) rb.isKinematic = true;
        if (TryGetComponent<Collider2D>(out var c))    c.isTrigger    = true;

        Debug.Log("🪄 Wand picked up into " + (_currentHoldPoint==flightHoldPoint?"flight":"ground") + " socket");
    }

    private void DropWand()
    {
        _isHeld    = false;
        _canPickup = false;

        // unparent
        transform.SetParent(null, true);

        // snap back above Luna
        if (_luna != null)
            transform.position = _luna.position + Vector3.up * 0.2f;

        // restore rotation (vertical)
        transform.rotation = Quaternion.Euler(0f, 0f, 90f);

        // restore scale just in case
        transform.localScale = _initialLocalScale;

        // re‑enable pickup icon, kinematic, trigger…
        if (pickupIcon != null) pickupIcon.SetActive(true);
        if (TryGetComponent<Rigidbody2D>(out var rb)) { rb.velocity = Vector2.zero; rb.angularVelocity = 0; rb.isKinematic = true; }
        if (TryGetComponent<Collider2D>(out var c))    c.isTrigger = true;

        Debug.Log("🔻 Wand dropped.");
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
*/