using UnityEngine;
using Cinemachine;
using System.Collections;
using System.Collections.Generic;

public class ButterflyFlyHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject luna;                     // ground‐state Luna
    public GameObject lunaInFlight;             // flight sprite, child of butterfly
    public Transform butterfly;
    public CinemachineVirtualCamera vCam;
    public Animator butterflyAnimator;
    public SpriteRenderer butterflyRenderer;
    public FollowAndFlip followAndFlip;         // only on ground
    public GameObject butterflySpeechBubble;
    public GameObject sparklePrefab;
    public AudioSource lunaFootstepAudio;


    [Header("Animation Speeds")]
    public float animationSpeedFlying = 2f;
    public float animationSpeedNormal = 1f;

    [Header("Movement & Jump")]
    public float baseSpeed        = 2.2f;
    public float bonusSpeed       = 0f;
    public float jumpForce        = 5f;
    public Vector3 dismountOffset = new Vector3(0f, -0.5f, 0f);

    [Header("Flight Timing")]
    public float flightDuration   = 5f;
    public float warningTime      = 1f;
    public float cooldownDuration = 3f;

    [Header("Extension Settings")]
    [Tooltip("How many seconds using one spore will extend your flight")]
    public float extendTime = 1f;

    [Header("Butterfly Colors")]
    public Color normalColor   = Color.white;
    public Color warningColor  = Color.yellow;
    public Color cooldownColor = Color.gray;

    [Header("Spore Attach Points")]
    public Transform sporeHoldPoint;
    public Transform flightSporeAttachPoint;

    [Header("Tea Rose Pollen")]
    public GameObject teaRosePollenFX;
    public int   teaRosePollenCount   = 0;
    public float speedBoostPerPollen  = 0.2f;
    public int   pollenThreshold      = 6;
    public float maxBonusSpeed = 1f;


    [HideInInspector]
    public bool justDismounted = false;

    [Header("Solid Colliders to Disable in Flight")]
    public Collider2D[] butterflySolidColliders;
    public Collider2D[] lunaSolidColliders;

    [Header("Flower Hold Points")]
    public Transform groundFlowerHoldPoint;    // assign this in the Inspector
    public Transform flightFlowerHoldPoint;    // assign this child of the butterfly

    [Header("Air‑Interact Settings")]
    [Tooltip("How far the butterfly can reach to detect flowers or gardens in flight.")]
    public float airInteractRadius = 0.3f;

    [Header("Mount Settings")]
    [Tooltip("How close Luna must be to the butterfly to mount.")]
    public float maxMountDistance = 2f;  // tweak this in the Inspector

    [Header("Wand Hold Points")]
    public Transform groundWandHoldPoint; // drag the empty on Luna here
    public Transform flightWandHoldPoint; // child of butterfly where wand will sit in flight
    public FlightBoundaryLimiter flightLimiter;

    [Header("Pollen Hold Points")]
    public Transform anemonePollenGroundHoldPoint;
    public Transform anemonePollenFlightHoldPoint;
    public Transform foxglovePollenGroundHoldPoint;
    public Transform foxglovePollenFlightHoldPoint;

    // internals
    private LunaSporeSystem _spore;
    private FlowerHolder    _holder;
    private Rigidbody2D     _rbLuna;
    private float           _lunaGravity;
    private SpriteRenderer  _sprLuna;
    private Coroutine _flashWarningCoroutine;
    private float  _flightTimer;
    public bool   _isFlying;
    private bool   _warningTriggered;
    private bool   _canExtend;
    private bool   _hasTempBoost;
    private float  _nextTempBoost;
    private bool   _inCooldown;

    private Vector3 _butterflyOrigScale;
    private Vector3 _lunaOrigScale;
    private bool    _isFacingRight = true;
    private GameObject _wandObj;


    void Start()
    {
        // cache systems
        _spore   = luna.GetComponent<LunaSporeSystem>();
        _holder  = luna.GetComponent<FlowerHolder>();
        _rbLuna  = luna.GetComponent<Rigidbody2D>();
        if (_rbLuna != null) _lunaGravity = _rbLuna.gravityScale;
        _sprLuna = luna.GetComponent<SpriteRenderer>();

        // initial spore attach
        _spore.attachPoint = sporeHoldPoint;

        // visuals
        _sprLuna.enabled       = true;
        lunaInFlight.SetActive(false);
        sparklePrefab?.SetActive(false);
        teaRosePollenFX?.SetActive(false);

        // cache absolute original scales
        _butterflyOrigScale = butterfly.localScale;
        _butterflyOrigScale.x = Mathf.Abs(_butterflyOrigScale.x);
        _lunaOrigScale = lunaInFlight.transform.localScale;
        _lunaOrigScale.x = Mathf.Abs(_lunaOrigScale.x);

        // start facing right
        _isFacingRight = true;

        // set up flower holder’s ground hold‑point
        _holder.holdPoint = groundFlowerHoldPoint;

         // grab the wand object so we can reparent it
        var wand = FindObjectOfType<LunariaWandAttractor>();
        if (wand != null)
            _wandObj = wand.gameObject;

    }

    void Update()
    {
        // Mount / Dismount on F
        // Mount / Dismount on F (with distance check)
        if (Input.GetKeyDown(KeyCode.F) && !_inCooldown)
        {
            if (_isFlying)
            {
                Dismount(false);
            }
            else
            {
                // only allow mounting when Luna is near the butterfly
                float dist = Vector2.Distance(luna.transform.position, butterfly.position);
                if (dist <= maxMountDistance)
                {
                    Mount();
                }
                else
                {
                    // optional feedback if you want to debug or show "too far"
                    Debug.Log($"Too far to mount (distance {dist:F1} > {maxMountDistance:F1})");
                }
            }
            return;
        }


        if (!_isFlying) return;
        // Flight controls
        Vector2 dir = HandleMovement();
        HandleFacing(dir);
        HandleTimer();
    

        // R: spawn + extend + auto‑destroy
        if (Input.GetKeyDown(KeyCode.R))
        {
            GameObject sp = _spore.CreateSpore();
            if (sp != null)
            {
                ExtendFlight();
                Destroy(sp, 0.1f);      // Unity will destroy it for you in 0.1 seconds
            }
        }

        // Jump off
        if (Input.GetButtonDown("Jump"))
            Dismount(true);

        // Mid‑air interact
        if (Input.GetKeyDown(KeyCode.X))
            HandleAirInteract();
    }

    Vector2 HandleMovement()
    {
        Vector2 dir = Vector2.zero;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  dir.x = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) dir.x = +1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))    dir.y = +1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))  dir.y = -1f;

        if (dir != Vector2.zero)
        {
            dir.Normalize();
            float speed = baseSpeed + bonusSpeed + (_hasTempBoost ? _nextTempBoost : 0f);
            butterfly.position += (Vector3)(dir * speed * Time.deltaTime);
        }
        return dir;
    }

    void HandleFacing(Vector2 dir)
    {
        // flip based on movement direction
        if (dir.x > 0.01f)       _isFacingRight = true;
        else if (dir.x < -0.01f) _isFacingRight = false;

        float sign = _isFacingRight ? +1f : -1f;
        butterfly.localScale = new Vector3(
            _butterflyOrigScale.x * sign,
            _butterflyOrigScale.y,
            _butterflyOrigScale.z
        );
    }

    void Mount()
    {
        justDismounted    = false;
        _isFlying         = true;
        _canExtend        = false;
        _warningTriggered = false;
        _flightTimer      = 0f;

        // disable the system that would spawn its own spores
        _spore.enabled = false;
        
        // switch visuals into flight mode
        _sprLuna.enabled = false;
        lunaInFlight.SetActive(true);
        if (lunaFootstepAudio != null) lunaFootstepAudio.enabled = false;
        if (flightLimiter != null) flightLimiter.enabled = true;
        _spore.attachPoint = flightSporeAttachPoint;

        // teleport any already‐held spore into the butterfly’s hand
        if (sporeHoldPoint.childCount > 0)
        {
            var sp = sporeHoldPoint.GetChild(0);
            sp.SetParent(flightSporeAttachPoint, true);
            sp.localPosition = Vector3.zero;
            sp.localRotation = Quaternion.identity;
        }

        // disable physics colliders so Luna “rides” smoothly
        foreach (var c in butterflySolidColliders) c.enabled = false;
        foreach (var c in lunaSolidColliders)      c.enabled = false;

        // have the camera follow the butterfly
        vCam.Follow               = butterfly;
        butterflySpeechBubble?.SetActive(false);
        followAndFlip.enabled     = false;
        sparklePrefab?.SetActive(true);

        // speed up flight animation and reset tint
        butterflyAnimator.speed  = animationSpeedFlying;
        butterflyRenderer.color  = normalColor;

        // switch the flower‑holder into flight position
        _holder.holdPoint = flightFlowerHoldPoint;

        // if we were already holding a flower, reparent it instantly
        if (_holder.HasFlower)
        {
            var f = _holder.GetHeldFlower();
            // <-- worldPositionStays = true preserves worldScale
            f.transform.SetParent(flightFlowerHoldPoint, true);
            f.transform.localPosition = Vector3.zero;
            f.transform.localRotation = Quaternion.identity;
        }

        // if the wand is currently parented to Luna, snap it onto the butterfly
        if (_wandObj.transform.parent == luna.transform)
        {
            _wandObj.transform.SetParent(flightWandHoldPoint, true);
            _wandObj.transform.localPosition = Vector3.zero;
            _wandObj.transform.localRotation = Quaternion.identity;
        }

       // Anemone pollen to flight
        if (anemonePollenGroundHoldPoint.childCount > 0)
        {
            var icon = anemonePollenGroundHoldPoint.GetChild(0);
            icon.SetParent(anemonePollenFlightHoldPoint, true);
            icon.localPosition = Vector3.zero;
            icon.localRotation = Quaternion.identity;
            icon.gameObject.SetActive(true);
        }

        // Foxglove pollen to flight
        if (foxglovePollenGroundHoldPoint.childCount > 0)
        {
            var icon = foxglovePollenGroundHoldPoint.GetChild(0);
            icon.SetParent(foxglovePollenFlightHoldPoint, true);
            icon.localPosition = Vector3.zero;
            icon.localRotation = Quaternion.identity;
            icon.gameObject.SetActive(true);
        }

    }

   void Dismount(bool jumpOff)
    {
        // Snap Luna back under the butterfly
        luna.transform.position = butterfly.position + dismountOffset;

        _isFlying   = false;
        _inCooldown = true;
        Invoke(nameof(EndCooldown), cooldownDuration);

        if (flightLimiter != null) flightLimiter.enabled = false;

        // Enforce the correct facing on the butterfly
        float sign = _isFacingRight ? +1f : -1f;
        butterfly.localScale = new Vector3(
            _butterflyOrigScale.x * sign,
            _butterflyOrigScale.y,
            _butterflyOrigScale.z
        );

        // Restore spore system
        _spore.enabled      = true;
        _spore.attachPoint  = sporeHoldPoint;

        // Send that same spore back down to the ground hold‑point
        if (flightSporeAttachPoint.childCount > 0)
        {
            var sp = flightSporeAttachPoint.GetChild(0);
            sp.SetParent(sporeHoldPoint, true);
            sp.localPosition = Vector3.zero;
            sp.localRotation = Quaternion.identity;
        }

        // Restore flower‑holder to ground position
        _holder.holdPoint = groundFlowerHoldPoint;

        // If still holding a flower, reparent back (preserve world scale)
        if (_holder.HasFlower)
        {
            var f = _holder.GetHeldFlower();
            f.transform.SetParent(groundFlowerHoldPoint, true);
            f.transform.localPosition = Vector3.zero;
            f.transform.localRotation = Quaternion.identity;
        }

        // If the wand is currently parented to Luna, snap it onto the butterfly
        if (_wandObj.transform.parent == luna.transform)
        {
            _wandObj.transform.SetParent(flightWandHoldPoint, true);
            _wandObj.transform.localPosition = Vector3.zero;
            _wandObj.transform.localRotation = Quaternion.identity;
        }

        // Anemone pollen back to ground
        if (anemonePollenFlightHoldPoint.childCount > 0)
        {
            var icon = anemonePollenFlightHoldPoint.GetChild(0);
            icon.SetParent(anemonePollenGroundHoldPoint, true);
            icon.localPosition = Vector3.zero;
            icon.localRotation = Quaternion.identity;
            icon.gameObject.SetActive(false); // optional: hide after dismount
        }

        // Foxglove pollen back to ground
        if (foxglovePollenFlightHoldPoint.childCount > 0)
        {
            var icon = foxglovePollenFlightHoldPoint.GetChild(0);
            icon.SetParent(foxglovePollenGroundHoldPoint, true);
            icon.localPosition = Vector3.zero;
            icon.localRotation = Quaternion.identity;
            icon.gameObject.SetActive(false); // optional: hide after dismount
        }

        // Switch visuals back to ground mode
        lunaInFlight.SetActive(false);
        _sprLuna.enabled = true;

        if (lunaFootstepAudio != null) lunaFootstepAudio.enabled = true;

        // Re-enable physics colliders
        foreach (var c in butterflySolidColliders) c.enabled = true;
        foreach (var c in lunaSolidColliders)      c.enabled = true;

        // Camera back to Luna
        vCam.Follow = luna.transform;
        sparklePrefab?.SetActive(false);

        // Re-enable FollowAndFlip one frame later
        StartCoroutine(ReenableFollowNextFrame());

        // Reset flight animation & tint to cooldown
        butterflyAnimator.speed  = animationSpeedNormal;
        butterflyRenderer.color  = cooldownColor;

        // Apply a jump impulse if requested
        if (jumpOff && _rbLuna != null)
        {
            _rbLuna.velocity = Vector2.zero;
            _rbLuna.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    void ExtendFlight()
    {
       if (_flashWarningCoroutine != null)
        {
            StopCoroutine(_flashWarningCoroutine);
            _flashWarningCoroutine = null;
        }

        // Push the timer back by extendTime seconds
        _flightTimer = Mathf.Max(0f, _flightTimer - extendTime);

        // Reset visuals & warning so you get a fresh warning window
        butterflyRenderer.color = normalColor;
        sparklePrefab?.SetActive(true);
        _warningTriggered = false;

        _canExtend = false;
    }

    IEnumerator FlashWarning()
    {
        for (int i = 0; i < 3; i++)
        {
            // flash warning…
            butterflyRenderer.color = warningColor;
            yield return new WaitForSeconds(0.2f);
            // back to normal…
            butterflyRenderer.color = normalColor;
            yield return new WaitForSeconds(0.2f);
        }
        // at end, stick on warning until actual dismount
        butterflyRenderer.color = warningColor;
    }

    void EndCooldown()
    {
        _inCooldown             = false;
        butterflyRenderer.color = normalColor;
    }

    void HandleAirInteract()
    {
        var hits = Physics2D.OverlapCircleAll(butterfly.position, airInteractRadius);
        foreach (var col in hits)
        {
            if (col.CompareTag("Flower") && !_holder.HasFlower)
            {
                _holder.PickUpFlower(col.gameObject);
                return;
            }
            var spot = col.GetComponent<GardenSpot>();
            if (spot != null && _holder.HasFlower)
            {
                var f = _holder.GetHeldFlower();
                f.transform.SetParent(spot.transform, false);
                f.transform.localPosition = Vector3.zero;
                _holder.DropFlower();
                var spr = f.GetComponent<SproutAndLightManager>();
                if (spr != null) spr.isPlanted = true;
                return;
            }
        }
    }

    public void ApplyTeaRosePollen(float speedBoost, int threshold)
    {
        teaRosePollenCount++;
        _hasTempBoost          = true;
        _nextTempBoost         = speedBoostPerPollen;
        teaRosePollenFX?.SetActive(true);

        if (teaRosePollenCount >= pollenThreshold)
        {
            bonusSpeed          += speedBoostPerPollen;
            bonusSpeed = Mathf.Min(bonusSpeed, maxBonusSpeed);
            teaRosePollenCount  = 0;
             // boost is now permanent, so turn the FX off:
        _hasTempBoost = false;
        teaRosePollenFX?.SetActive(false);
        }
    }

    IEnumerator ReenableFollowNextFrame()
    {
        // wait until the next frame
        yield return null;
        followAndFlip.enabled = true;
    }

    void HandleTimer()
    {
        _flightTimer += Time.deltaTime;

        if (!_warningTriggered && _flightTimer >= flightDuration - warningTime)
        {
            _warningTriggered = true;
            _canExtend        = true;
            sparklePrefab?.SetActive(false);
            _flashWarningCoroutine = StartCoroutine(FlashWarning());
        }

        if (_flightTimer >= flightDuration)
        {
            Dismount(false);
        }
    }

}
