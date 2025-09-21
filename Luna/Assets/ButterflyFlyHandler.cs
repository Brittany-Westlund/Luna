using UnityEngine;
using Cinemachine;
using System.Collections;

public class ButterflyFlyHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject luna;                     // ground-state Luna
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
    public float maxBonusSpeed        = 1f;

    [Header("Buttercup Pollen")]
    public GameObject buttercupPollenIcon;     // prefab or child icon
    public Transform buttercupPollenHoldPoint; // optional anchor
    public float buttercupIconDuration = 2f;

    [HideInInspector] public bool justDismounted = false;

    [Header("Solid Colliders to Disable in Flight")]
    public Collider2D[] butterflySolidColliders;
    public Collider2D[] lunaSolidColliders;

    [Header("Flower Hold Points")]
    public Transform groundFlowerHoldPoint;
    public Transform flightFlowerHoldPoint;

    [Header("Air-Interact Settings")]
    [Tooltip("How far the butterfly can reach to detect flowers/gardens in flight.")]
    public float airInteractRadius = 0.3f;

    [Header("Mount Settings")]
    [Tooltip("How close Luna must be to the butterfly to mount.")]
    public float maxMountDistance = 2f;

    [Header("Wand Hold Points")]
    public Transform groundWandHoldPoint; // empty on Luna
    public Transform flightWandHoldPoint; // child of butterfly

    [Header("Limits")]
    public FlightBoundaryLimiter flightLimiter;

    [Header("Pollen Hold Points")]
    public Transform anemonePollenGroundHoldPoint;
    public Transform anemonePollenFlightHoldPoint;
    public Transform foxglovePollenGroundHoldPoint;
    public Transform foxglovePollenFlightHoldPoint;
    public Transform goldenrodPollenGroundHoldPoint;
    public Transform goldenrodPollenFlightHoldPoint;

    // --- internals ---
    public  LunaSporeSystem _spore;
    private FlowerHolder    _holder;
    private Rigidbody2D     _rbLuna;
    private float           _lunaGravity;
    private SpriteRenderer  _sprLuna;
    private Coroutine       _flashWarningCoroutine;
    private float           _flightTimer;
    public  bool            _isFlying;
    private bool            _warningTriggered;
    private bool            _canExtend;
    private bool            _hasTempBoost;
    private float           _nextTempBoost;
    private bool            _inCooldown;
    private Vector3         _butterflyOrigScale;
    private Vector3         _lunaOrigScale;
    private bool            _isFacingRight = true;
    private GameObject      _wandObj;
    private ButterflyFatigue _fatigue;

    void Start()
    {
        // Safeguard lookups
        if (!luna) luna = GameObject.FindWithTag("Player");
        if (!butterfly) butterfly = transform;

        // Cache systems off Luna
        if (luna)
        {
            _spore   = luna.GetComponent<LunaSporeSystem>();
            _holder  = luna.GetComponent<FlowerHolder>();
            _rbLuna  = luna.GetComponent<Rigidbody2D>();
            _sprLuna = luna.GetComponent<SpriteRenderer>();
        }

        if (_rbLuna) _lunaGravity = _rbLuna.gravityScale;

        // Initial spore attach
        if (_spore && sporeHoldPoint) _spore.attachPoint = sporeHoldPoint;

        // Visuals
        if (_sprLuna) _sprLuna.enabled = true;
        if (lunaInFlight) lunaInFlight.SetActive(false);
        if (sparklePrefab) sparklePrefab.SetActive(false);
        if (teaRosePollenFX) teaRosePollenFX.SetActive(false);

        // Cache absolute original scales
        if (butterfly) { _butterflyOrigScale = butterfly.localScale; _butterflyOrigScale.x = Mathf.Abs(_butterflyOrigScale.x); }
        if (lunaInFlight) { _lunaOrigScale = lunaInFlight.transform.localScale; _lunaOrigScale.x = Mathf.Abs(_lunaOrigScale.x); }

        _isFacingRight = true;

        // Flower holder ground anchor
        if (_holder && groundFlowerHoldPoint) _holder.holdPoint = groundFlowerHoldPoint;

        // Wand (optional)
        var wand = FindObjectOfType<LunariaWandAttractor>();
        if (wand) _wandObj = wand.gameObject;

        _fatigue = GetComponent<ButterflyFatigue>();
    }

    void Update()
    {
        // Mount / Dismount (F)
        if (Input.GetKeyDown(KeyCode.F) && !_inCooldown)
        {
            if (_isFlying)
            {
                Dismount(false);
            }
            else
            {
                if (luna && butterfly)
                {
                    float dist = Vector2.Distance(luna.transform.position, butterfly.position);
                    if (dist <= maxMountDistance) Mount();
                    else Debug.Log($"Too far to mount (distance {dist:F1} > {maxMountDistance:F1})");
                }
            }
            return;
        }

        if (!_isFlying) return;

        // Flight controls
        Vector2 dir = HandleMovement();
        HandleFacing(dir);
        HandleTimer();

        if (_fatigue != null && _fatigue.IsExhausted() && _isFlying)
        {
            Debug.Log("💥 Fatigue maxed out during flight — forcing dismount.");
            Dismount(false);
            return;
        }

        // R: spawn + extend + auto-destroy
        if (Input.GetKeyDown(KeyCode.R) && _spore)
        {
            GameObject sp = _spore.CreateSpore();
            if (sp != null)
            {
                ExtendFlight();
                Destroy(sp, 0.1f);
            }
        }

        // Jump off
        if (Input.GetButtonDown("Jump"))
            Dismount(true);

        // Mid-air interact
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

        if (dir != Vector2.zero && butterfly)
        {
            dir.Normalize();
            float speed = baseSpeed + bonusSpeed + (_hasTempBoost ? _nextTempBoost : 0f);
            butterfly.position += (Vector3)(dir * speed * Time.deltaTime);
        }
        return dir;
    }

    void HandleFacing(Vector2 dir)
    {
        if (dir.x >  0.01f) _isFacingRight = true;
        if (dir.x < -0.01f) _isFacingRight = false;

        if (butterfly)
        {
            float sign = _isFacingRight ? +1f : -1f;
            butterfly.localScale = new Vector3(
                _butterflyOrigScale.x * sign,
                _butterflyOrigScale.y,
                _butterflyOrigScale.z
            );
        }
    }

    void Mount()
    {
        if (_fatigue != null && _fatigue.WouldExceedFatigue())
        {
            Debug.Log("❌ Cannot mount — mounting would exceed fatigue limit.");
            return;
        }

        justDismounted    = false;
        _isFlying         = true;
        _canExtend        = false;
        _warningTriggered = false;
        _flightTimer      = 0f;

        // Disable spore system (prevents spawning while flying)
        if (_spore) _spore.enabled = false;

        // Switch visuals to flight
        if (_sprLuna) _sprLuna.enabled = false;
        if (lunaInFlight) lunaInFlight.SetActive(true);
        if (lunaFootstepAudio) lunaFootstepAudio.enabled = false;
        if (flightLimiter) flightLimiter.enabled = true;
        if (_spore && flightSporeAttachPoint) _spore.attachPoint = flightSporeAttachPoint;

        // Move an already-held spore to the flight hand
        if (sporeHoldPoint && flightSporeAttachPoint && sporeHoldPoint.childCount > 0)
        {
            var sp = sporeHoldPoint.GetChild(0);
            sp.SetParent(flightSporeAttachPoint, true);
            sp.localPosition = Vector3.zero;
            sp.localRotation = Quaternion.identity;
        }

        // Colliders off for smooth ride
        ToggleColliders(butterflySolidColliders, false);
        ToggleColliders(lunaSolidColliders, false);

        // Camera follows butterfly
        if (vCam && butterfly) vCam.Follow = butterfly;

        if (butterflySpeechBubble) butterflySpeechBubble.SetActive(false);
        if (followAndFlip) followAndFlip.enabled = false;
        if (sparklePrefab) sparklePrefab.SetActive(true);

        if (butterflyAnimator) butterflyAnimator.speed = animationSpeedFlying;

        if (_fatigue != null)
        {
            _fatigue.ApplyFatigue(); // increments fatigue + sets fatigue color
            if (butterflyRenderer) Debug.Log($"[Mount] Applied fatigue. Current color: {butterflyRenderer.color}");
        }

        // Flower holder to flight position
        if (_holder && flightFlowerHoldPoint) _holder.holdPoint = flightFlowerHoldPoint;

        if (_holder != null && _holder.HasFlower && flightFlowerHoldPoint)
        {
            var f = _holder.GetHeldFlower();
            if (f)
            {
                f.transform.SetParent(flightFlowerHoldPoint, true);
                f.transform.localPosition = Vector3.zero;
                f.transform.localRotation = Quaternion.identity;
            }
        }

        // Wand → butterfly
        if (_wandObj && _wandObj.transform && flightWandHoldPoint && luna)
        {
            if (_wandObj.transform.parent == luna.transform)
            {
                _wandObj.transform.SetParent(flightWandHoldPoint, true);
                _wandObj.transform.localPosition = Vector3.zero;
                _wandObj.transform.localRotation = Quaternion.identity;
            }
        }

        // Pollen icons → flight
        MoveIfChildExists(anemonePollenGroundHoldPoint,  anemonePollenFlightHoldPoint);
        MoveIfChildExists(foxglovePollenGroundHoldPoint,  foxglovePollenFlightHoldPoint);
        MoveIfChildExists(goldenrodPollenGroundHoldPoint, goldenrodPollenFlightHoldPoint);
    }

    void Dismount(bool jumpOff)
    {
        if (!_isFlying) return;      // prevent double dismount
        _isFlying = false;

        // Snap Luna just below the butterfly
        if (luna && butterfly) luna.transform.position = butterfly.position + dismountOffset;

        _inCooldown = true;
        Invoke(nameof(EndCooldown), cooldownDuration);

        if (flightLimiter) flightLimiter.enabled = false;

        // Keep butterfly facing consistent
        if (butterfly)
        {
            float sign = _isFacingRight ? +1f : -1f;
            butterfly.localScale = new Vector3(
                _butterflyOrigScale.x * sign,
                _butterflyOrigScale.y,
                _butterflyOrigScale.z
            );
        }

        // Restore spore system
        if (_spore)
        {
            _spore.enabled = true;
            if (sporeHoldPoint) _spore.attachPoint = sporeHoldPoint;
        }

        // Return spore to ground hand
        if (flightSporeAttachPoint && sporeHoldPoint && flightSporeAttachPoint.childCount > 0)
        {
            var sp = flightSporeAttachPoint.GetChild(0);
            sp.SetParent(sporeHoldPoint, true);
            sp.localPosition = Vector3.zero;
            sp.localRotation = Quaternion.identity;
        }

        // Flower holder back to ground
        if (_holder && groundFlowerHoldPoint) _holder.holdPoint = groundFlowerHoldPoint;

        if (_holder && _holder.HasFlower && groundFlowerHoldPoint)
        {
            var f = _holder.GetHeldFlower();
            if (f)
            {
                f.transform.SetParent(groundFlowerHoldPoint, true);
                f.transform.localPosition = Vector3.zero;
                f.transform.localRotation = Quaternion.identity;
            }
        }

        // (Your original logic kept the wand on the flight point; left as-is)
        if (_wandObj && _wandObj.transform && flightWandHoldPoint)
        {
            _wandObj.transform.SetParent(flightWandHoldPoint, true);
            _wandObj.transform.localPosition = Vector3.zero;
            _wandObj.transform.localRotation = Quaternion.identity;
        }

        // Pollen icons → ground
        MoveIfChildExists(anemonePollenFlightHoldPoint, anemonePollenGroundHoldPoint);
        MoveIfChildExists(foxglovePollenFlightHoldPoint, anemonePollenGroundHoldPoint);     // if that was a typo, swap target to foxglove ground
        MoveIfChildExists(goldenrodPollenFlightHoldPoint, goldenrodPollenGroundHoldPoint);

        // Visuals back to ground mode
        if (lunaInFlight) lunaInFlight.SetActive(false);
        if (_sprLuna) _sprLuna.enabled = true;
        if (lunaFootstepAudio) lunaFootstepAudio.enabled = true;

        // Re-enable physics colliders
        ToggleColliders(butterflySolidColliders, true);
        ToggleColliders(lunaSolidColliders, true);

        // Camera back to Luna
        if (vCam && luna) vCam.Follow = luna.transform;
        if (sparklePrefab) sparklePrefab.SetActive(false);

        // Re-enable FollowAndFlip next frame
        StartCoroutine(ReenableFollowNextFrame());

        if (butterflyAnimator) butterflyAnimator.speed = animationSpeedNormal;
        if (butterflyRenderer) butterflyRenderer.color = cooldownColor;

        // Optional jump impulse
        if (jumpOff && _rbLuna)
        {
            _rbLuna.velocity = Vector2.zero;
            _rbLuna.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (butterflyRenderer) butterflyRenderer.color = cooldownColor;
    }

    void ExtendFlight()
    {
        if (_flashWarningCoroutine != null)
        {
            StopCoroutine(_flashWarningCoroutine);
            _flashWarningCoroutine = null;
        }

        // Push timer back by extendTime seconds
        _flightTimer = Mathf.Max(0f, _flightTimer - extendTime);

        // Reset visuals & warning window
        if (_fatigue != null && butterflyRenderer) butterflyRenderer.color = _fatigue.GetFatigueColor();
        if (sparklePrefab) sparklePrefab.SetActive(true);
        _warningTriggered = false;
        _canExtend = false;
    }

    IEnumerator FlashWarning()
    {
        for (int i = 0; i < 3; i++)
        {
            if (butterflyRenderer) butterflyRenderer.color = warningColor;
            yield return new WaitForSeconds(0.2f);
            if (_fatigue != null && butterflyRenderer) butterflyRenderer.color = _fatigue.GetFatigueColor();
            yield return new WaitForSeconds(0.2f);
        }
        if (_fatigue != null && butterflyRenderer) butterflyRenderer.color = _fatigue.GetFatigueColor();
    }

    void EndCooldown()
    {
        _inCooldown = false;
        if (_fatigue != null && butterflyRenderer) butterflyRenderer.color = _fatigue.GetFatigueColor();
    }

    void HandleAirInteract()
    {
        if (!butterfly) return;

        var hits = Physics2D.OverlapCircleAll(butterfly.position, airInteractRadius);
        foreach (var col in hits)
        {
            if (_holder == null) break;

            if (col.CompareTag("Flower") && !_holder.HasFlower)
            {
                _holder.PickUpFlower(col.gameObject);
                return;
            }

            var spot = col.GetComponent<GardenSpot>();
            if (spot != null && _holder.HasFlower)
            {
                var f = _holder.GetHeldFlower();
                if (f)
                {
                    f.transform.SetParent(spot.transform, false);
                    f.transform.localPosition = Vector3.zero;
                    _holder.DropFlower();
                    var spr = f.GetComponent<SproutAndLightManager>();
                    if (spr != null) spr.isPlanted = true;
                }
                return;
            }
        }
    }

    public void ApplyTeaRosePollen(float speedBoost, int threshold)
    {
        teaRosePollenCount++;
        _hasTempBoost  = true;
        _nextTempBoost = speedBoostPerPollen;
        if (teaRosePollenFX) teaRosePollenFX.SetActive(true);

        if (teaRosePollenCount >= pollenThreshold)
        {
            bonusSpeed = Mathf.Min(bonusSpeed + speedBoostPerPollen, maxBonusSpeed);
            teaRosePollenCount = 0;
            _hasTempBoost = false;
            if (teaRosePollenFX) teaRosePollenFX.SetActive(false);
        }
    }

    public void ShowButtercupPollenIcon()
    {
        if (!buttercupPollenIcon) return;
        StopCoroutine(nameof(ButtercupIconRoutine));
        StartCoroutine(ButtercupIconRoutine());
    }

    private IEnumerator ButtercupIconRoutine()
    {
        buttercupPollenIcon.SetActive(true);
        yield return new WaitForSeconds(buttercupIconDuration);
        buttercupPollenIcon.SetActive(false);
    }

    IEnumerator ReenableFollowNextFrame()
    {
        yield return null;
        if (followAndFlip) followAndFlip.enabled = true;
    }

    void HandleTimer()
    {
        _flightTimer += Time.deltaTime;

        if (!_warningTriggered && _flightTimer >= flightDuration - warningTime)
        {
            _warningTriggered = true;
            _canExtend        = true;
            if (sparklePrefab) sparklePrefab.SetActive(false);
            _flashWarningCoroutine = StartCoroutine(FlashWarning());
        }

        if (_flightTimer >= flightDuration)
        {
            Dismount(false);
        }
    }

    // --- helpers ---
    void ToggleColliders(Collider2D[] cols, bool state)
    {
        if (cols == null) return;
        foreach (var c in cols) if (c) c.enabled = state;
    }

    void MoveIfChildExists(Transform from, Transform to)
    {
        if (!from || !to) return;
        if (from.childCount > 0)
        {
            var t = from.GetChild(0);
            t.SetParent(to, true);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
        }
    }
}



/* using UnityEngine;
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

    [Header("Buttercup Pollen")]
    public GameObject buttercupPollenIcon;     // <-- drag in the prefab or child icon
    public Transform buttercupPollenHoldPoint; // <-- this can be a small child Transform of the butterfly
    public float buttercupIconDuration = 2f;   // <-- how long it stays visible

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
    public Transform goldenrodPollenGroundHoldPoint;
    public Transform goldenrodPollenFlightHoldPoint;

    // internals
    public LunaSporeSystem _spore;
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
    private ButterflyFatigue _fatigue;


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
            
        _fatigue = GetComponent<ButterflyFatigue>();

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
        if (_fatigue != null && _fatigue.IsExhausted() && _isFlying)
        {
            Debug.Log("💥 Fatigue maxed out during flight — forcing dismount.");
            Dismount(false);
            return;
        }

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
      if (_fatigue != null && _fatigue.WouldExceedFatigue())
        {
            Debug.Log("❌ Cannot mount — mounting would exceed fatigue limit.");
            return;
        }
       
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

        // speed up flight animation
        butterflyAnimator.speed = animationSpeedFlying;

      if (_fatigue != null)
        {
            _fatigue.ApplyFatigue(); // increments fatigue + applies fatigue color
            Debug.Log($"[Mount] Applied fatigue. Current color: {butterflyRenderer.color}");
        }

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
        // only run this if we actually have a wand
        if (_wandObj != null && _wandObj.transform != null)
        {
            if (_wandObj.transform.parent == luna.transform)
            {
                _wandObj.transform.SetParent(flightWandHoldPoint, true);
                _wandObj.transform.localPosition = Vector3.zero;
                _wandObj.transform.localRotation = Quaternion.identity;
            }
        }

        if (anemonePollenGroundHoldPoint.childCount > 0)
            {
                var icon = anemonePollenGroundHoldPoint.GetChild(0);
                icon.SetParent(anemonePollenFlightHoldPoint, true);
                icon.localPosition = Vector3.zero;
                icon.localRotation = Quaternion.identity;
            }

        if (foxglovePollenGroundHoldPoint.childCount > 0)
        {
            var icon = foxglovePollenGroundHoldPoint.GetChild(0);
            icon.SetParent(foxglovePollenFlightHoldPoint, true);
            icon.localPosition = Vector3.zero;
            icon.localRotation = Quaternion.identity;
        }

        if (goldenrodPollenGroundHoldPoint.childCount > 0)
        {
            var icon = goldenrodPollenGroundHoldPoint.GetChild(0);
            icon.SetParent(goldenrodPollenFlightHoldPoint, true);
            icon.localPosition = Vector3.zero;
            icon.localRotation = Quaternion.identity;
        }

    }

   void Dismount(bool jumpOff)
    {
       if (!_isFlying) return; // prevent double dismount
        _isFlying = false;

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

        // If the wand exists and is currently parented to Luna, snap it onto the butterfly
        if (_wandObj != null && _wandObj.transform.parent == luna.transform)
        {
            _wandObj.transform.SetParent(flightWandHoldPoint, true);
            _wandObj.transform.localPosition = Vector3.zero;
            _wandObj.transform.localRotation = Quaternion.identity;
        }

        if (anemonePollenFlightHoldPoint.childCount > 0)
        {
            var icon = anemonePollenFlightHoldPoint.GetChild(0);
            icon.SetParent(anemonePollenGroundHoldPoint, true);
            icon.localPosition = Vector3.zero;
            icon.localRotation = Quaternion.identity;
        }

        if (foxglovePollenFlightHoldPoint.childCount > 0)
        {
            var icon = foxglovePollenFlightHoldPoint.GetChild(0);
            icon.SetParent(foxglovePollenGroundHoldPoint, true);
            icon.localPosition = Vector3.zero;
            icon.localRotation = Quaternion.identity;
        }

        if (goldenrodPollenFlightHoldPoint.childCount > 0)
        {
            var icon = goldenrodPollenFlightHoldPoint.GetChild(0);
            icon.SetParent(goldenrodPollenGroundHoldPoint, true);
            icon.localPosition = Vector3.zero;
            icon.localRotation = Quaternion.identity;
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

       // Set to cooldown color right now
        butterflyRenderer.color = cooldownColor;

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
        butterflyRenderer.color = _fatigue.GetFatigueColor();
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
            // back to fatigue…
            butterflyRenderer.color = _fatigue.GetFatigueColor();
            yield return new WaitForSeconds(0.2f);
        }
        // at end, return to fatigue color until actual dismount
        butterflyRenderer.color = _fatigue.GetFatigueColor();

    }

    void EndCooldown()
    {
        _inCooldown = false;

        
       if (_fatigue != null)
        {
            butterflyRenderer.color = _fatigue.GetFatigueColor();
        }

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

    public void ShowButtercupPollenIcon()
    {
        if (buttercupPollenIcon == null) return;

        StopCoroutine(nameof(ButtercupIconRoutine)); // ensures clean restart if it's already running
        StartCoroutine(ButtercupIconRoutine());
    }

    private IEnumerator ButtercupIconRoutine()
    {
        buttercupPollenIcon.SetActive(true);
        yield return new WaitForSeconds(buttercupIconDuration);
        buttercupPollenIcon.SetActive(false);
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
*/