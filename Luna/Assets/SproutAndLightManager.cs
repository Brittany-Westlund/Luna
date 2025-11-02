using UnityEngine;
using System.Collections;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld, isPlanted, isPlayerNearby;

    [Header("Growth Settings")]
    public int maxGrowthStages = 3;
    public float growthIncrement = 0.1f;

    [Header("Spore Hint Icon (prefers child)")]
    public GameObject sporeHintPrefab;
    public string sporeChildName = "SporeIcon";
    public Vector3 sporeHintOffset = new Vector3(0f, 0.5f, 0f);
    public float sporeHintScale = 1f;

    [Header("Light Hint Icon")]
    public GameObject lightHintPrefab;
    public Vector3 lightHintOffset = new Vector3(0f, 0.5f, 0f);
    public float lightHintScale = 1f;

    [Header("Lit Flower Sprite")]
    public SpriteRenderer litFlowerRenderer;

    [Header("Debug")]
    public bool debugLogs = false;

    private Vector3 _initialWorldScale;
    private Vector3 _initialWorldPos;
    private int _currentStage;
    private bool _isFullyGrown;
    public bool IsFullyGrown => _isFullyGrown;

    private bool hasBeenLit = false;
    private GameObject _sporeHintGO;
    private GameObject _lightHintGO;

    const float GARDEN_CHECK_RADIUS = 0.1f;

    void Awake()
    {
        _initialWorldScale = transform.lossyScale;
        _initialWorldPos = transform.position;
        _currentStage = 0;
        _isFullyGrown = false;

        if (litFlowerRenderer != null)
            litFlowerRenderer.enabled = false;

        // locate existing spore child
        var child = transform.Find(sporeChildName);
        if (child != null)
        {
            _sporeHintGO = child.gameObject;
            _sporeHintGO.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNearby = true;
        TryShowHint(force: true);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        TryShowHint(force: true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNearby = false;
        HideSporeHint();
        HideLightHint();
    }

    bool IsInOrNearGarden()
    {
        for (Transform t = transform; t != null; t = t.parent)
            if (t.CompareTag("Garden")) return true;

        var hits = Physics2D.OverlapCircleAll(transform.position, GARDEN_CHECK_RADIUS);
        foreach (var c in hits)
            if (c.CompareTag("Garden")) return true;

        return false;
    }

    void TryShowHint(bool force = false)
    {
        if (!isPlayerNearby || isHeld || !IsInOrNearGarden())
        {
            HideSporeHint();
            HideLightHint();
            return;
        }

        if (!_isFullyGrown)
            ShowSporeHint(force);
        else
            ShowLightHint(force);
    }

    public void ResetOnGrowth()
    {
        if (!IsInOrNearGarden()) return;
        if (_isFullyGrown) return;

        isPlanted = true;

// Grow first so collider & transform are settled
_currentStage = Mathf.Min(_currentStage + 1, maxGrowthStages);
float targetScale = _initialWorldScale.x + growthIncrement * _currentStage;
transform.localScale = Vector3.one * targetScale;

// 🌱 Now safely show the spore hint
StartCoroutine(ShowSporeHintNextFrame());
ForceShowSporeHint();


        // Continue with growth logic
        _currentStage = Mathf.Min(_currentStage + 1, maxGrowthStages);

        // grow flower only
        targetScale = _initialWorldScale.x + growthIncrement * _currentStage;
        transform.localScale = Vector3.one * targetScale;

        if (_currentStage >= maxGrowthStages)
        {
            _isFullyGrown = true;
            HideSporeHint();
            if (isPlayerNearby) ShowLightHint(true);
        }
        else
        {
            if (isPlayerNearby) ShowSporeHint(true);
        }
    }
    
   private IEnumerator ShowSporeHintNextFrame()
{
    // Wait one frame so Unity updates physics and trigger states
    yield return new WaitForFixedUpdate();


    // Check player and colliders manually
    var player = GameObject.FindGameObjectWithTag("Player");
    if (player == null) yield break;

    var playerCollider = player.GetComponent<Collider2D>();
    var myCollider = GetComponent<Collider2D>();

    if (playerCollider == null || myCollider == null) yield break;

    // If Luna is overlapping OR within a short distance
    bool closeEnough = myCollider.IsTouching(playerCollider) ||
                       Vector2.Distance(transform.position, player.transform.position) < 0.5f;

    if (closeEnough && !_isFullyGrown && !isHeld)
    {
        ShowSporeHint();
        if (debugLogs)
            Debug.Log($"🌱 Forced spore hint shown for {name} right after planting.");
    }
}

   
    /// <summary>
/// Forces the spore hint to show immediately if Luna is colliding,
/// regardless of stage or trigger state. Use this to guarantee visibility.
/// </summary>
public void ForceShowSporeHint()
{
    if (_isFullyGrown || isHeld) return;

    // Check if Luna is colliding with this flower
    var player = GameObject.FindGameObjectWithTag("Player");
    if (player == null) return;

    var myCollider = GetComponent<Collider2D>();
    var playerCollider = player.GetComponent<Collider2D>();
    if (myCollider == null || playerCollider == null) return;

    if (myCollider.IsTouching(playerCollider))
    {
        ShowSporeHint();
        if (debugLogs)
            Debug.Log($"🌱 Force showing spore hint for {name} because Luna is touching.");
    }
}


    void ShowSporeHint(bool force = false)
    {
        if (_currentStage >= maxGrowthStages || !IsInOrNearGarden()) { HideSporeHint(); return; }

        if (_sporeHintGO == null)
        {
            var child = transform.Find(sporeChildName);
            if (child != null)
                _sporeHintGO = child.gameObject;
            else if (sporeHintPrefab != null)
                _sporeHintGO = Instantiate(sporeHintPrefab, transform);
        }

        if (_sporeHintGO != null)
        {
            _sporeHintGO.SetActive(true);
            _sporeHintGO.transform.localPosition = sporeHintOffset;
            // Keep constant world size each frame (see LateUpdate)
            if (debugLogs && force)
                Debug.Log($"🌱 Spore hint active for {name}");
        }
    }

    void HideSporeHint()
    {
        if (_sporeHintGO != null) _sporeHintGO.SetActive(false);
    }

    void ShowLightHint(bool force = false)
    {
        if (!_isFullyGrown || lightHintPrefab == null || !IsInOrNearGarden()) return;

        if (_lightHintGO == null)
        {
            _lightHintGO = Instantiate(lightHintPrefab, transform);
            _lightHintGO.transform.localPosition = lightHintOffset;
            _lightHintGO.transform.localScale = Vector3.one * lightHintScale;
        }

        _lightHintGO.SetActive(true);
        if (debugLogs && force)
            Debug.Log($"🌕 Light hint active for {name}");
    }

    public void HideLightHint()
    {
        if (_lightHintGO != null) _lightHintGO.SetActive(false);
    }

    public void GiveLight()
    {
        if (!_isFullyGrown || !isPlayerNearby || !IsInOrNearGarden()) return;

        if (litFlowerRenderer != null)
            litFlowerRenderer.enabled = true;

        if (!hasBeenLit)
        {
            ScoreManager.Instance.AddPoint();
            hasBeenLit = true;
        }

        HideLightHint();
    }

    void LateUpdate()
    {
        // keep hint offsets & constant world size
        if (_sporeHintGO && _sporeHintGO.activeSelf)
        {
            _sporeHintGO.transform.position = transform.position + sporeHintOffset;
            _sporeHintGO.transform.rotation = Quaternion.identity;

            // counter-scale so it stays constant
            Vector3 invScale = new Vector3(
                1f / transform.lossyScale.x,
                1f / transform.lossyScale.y,
                1f / transform.lossyScale.z
            );
            _sporeHintGO.transform.localScale = Vector3.Scale(invScale, Vector3.one * sporeHintScale);
        }

        if (_lightHintGO && _lightHintGO.activeSelf)
        {
            _lightHintGO.transform.position = transform.position + lightHintOffset;
            _lightHintGO.transform.rotation = Quaternion.identity;

            Vector3 invScale = new Vector3(
                1f / transform.lossyScale.x,
                1f / transform.lossyScale.y,
                1f / transform.lossyScale.z
            );
            _lightHintGO.transform.localScale = Vector3.Scale(invScale, Vector3.one * lightHintScale);
        }
    }

    // Optional compatibility
    public void ResetInitialPosition()
    {
        _initialWorldPos = transform.position;
    }
    // Restore for callers in Garden* / Flower* managers
public void ClearAllHints()
{
    HideSporeHint();
    HideLightHint();
}

// Restore for TeapotReceiver
public void BrewFlower()
{
    if (debugLogs) Debug.Log($"[BrewFlower] {name} — hasBeenLit={hasBeenLit}");

    if (hasBeenLit)
    {
        // remove a point if already lit
        ScoreManager.Instance.points = Mathf.Max(0, ScoreManager.Instance.points - 1);
        ScoreManager.Instance.UpdatePointsText();
        hasBeenLit = false;

        if (litFlowerRenderer != null)
            litFlowerRenderer.enabled = false;
    }
}

}



/* using UnityEngine;
using System.Collections;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld, isPlanted, isPlayerNearby;

    [Header("Growth Settings")]
    public int   maxGrowthStages    = 3;
    public float growthIncrement    = 0.1f;

    [Header("Spore Hint Icon")]
    public GameObject sporeHintPrefab;
    public Vector3[]   hintOffsets;   // length ≥ maxGrowthStages
    public float[]     hintScales;    // length ≥ maxGrowthStages

    [Header("Light Hint Icon")]
    public GameObject lightHintPrefab;
    public Vector3    lightHintOffset;
    public float      lightHintScale  = 1f;

    [Header("Lit Flower Sprite")]
    public SpriteRenderer litFlowerRenderer;

    [Header("Debug")]
    public bool debugLogs = false;

    // Internals
    Vector3   _initialWorldScale;
    Vector3   _initialWorldPos;
    int       _currentStage;
    bool      _isFullyGrown;
    public bool IsFullyGrown => _isFullyGrown;

    private bool hasBeenLit = false;

    GameObject _sporeHintGO;
    Vector3    _sporeHintOffset;
    GameObject _lightHintGO;

    // Only‑one‑hint globals
    static SproutAndLightManager _activeSporeOwner;
    static SproutAndLightManager _activeLightOwner;

    // how close counts as "near" a garden if not parented under one
    const float GARDEN_CHECK_RADIUS = 0.1f;

    void Awake()
    {
        _initialWorldScale = transform.lossyScale;
        _initialWorldPos   = transform.position;
        _currentStage      = 0;
        _isFullyGrown      = false;

        if (litFlowerRenderer != null)
            litFlowerRenderer.enabled = false;

        HideSporeHint();
        HideLightHint();
    }

    void OnTriggerEnter2D(Collider2D other)
{
    if (!other.CompareTag("Player")) return;
    isPlayerNearby = true;

    // Re-show hint immediately upon entry
    TryShowHint();
}

void OnTriggerStay2D(Collider2D other)
{
    if (!other.CompareTag("Player")) return;

    // Safety: if something hid it mid-stay (e.g., a quick growth or reposition), show again
    if (!_isFullyGrown && (_sporeHintGO == null || !_sporeHintGO.activeSelf))
    {
        TryShowHint();
    }
    else if (_isFullyGrown && (_lightHintGO == null || !_lightHintGO.activeSelf))
    {
        TryShowHint();
    }
}

void OnTriggerExit2D(Collider2D other)
{
    if (!other.CompareTag("Player")) return;
    isPlayerNearby = false;

    // Hide both hints on exit
    HideSporeHint();
    HideLightHint();
}

    /// <summary>
    /// Central gatekeeper for hint‑spawning logic.
    /// </summary>
    void TryShowHint()
    {
        // only if player is nearby, not held, and in/near a Garden
        if (!isPlayerNearby || isHeld || !IsInOrNearGarden())
        {
            HideSporeHint();
            HideLightHint();
            return;
        }

        // show spore if not fully grown, otherwise light
        if (!_isFullyGrown)
            ShowSporeHint();
        else
            ShowLightHint();
    }

    /// <summary>
    /// Checks if this flower is parented under a "Garden" or near one by overlap.
    /// </summary>
    bool IsInOrNearGarden()
    {
        // 1) parent‐tag check
        for (Transform t = transform; t != null; t = t.parent)
            if (t.CompareTag("Garden"))
                return true;

        // 2) overlap check
        var hits = Physics2D.OverlapCircleAll(transform.position, GARDEN_CHECK_RADIUS);
        foreach (var c in hits)
            if (c.CompareTag("Garden"))
                return true;

        return false;
    }

    public void ResetInitialPosition()
{
    _initialWorldPos = transform.position;
}


   public void ResetOnGrowth()
{
    if (!IsInOrNearGarden())
    {
        Debug.Log("❗ Cannot grow flower outside of a garden.");
        return;
    }

    if (_isFullyGrown) return;

    isPlanted = true;
    _currentStage = Mathf.Min(_currentStage + 1, maxGrowthStages);

    // 🌱 Pivot-based growth: scale only
    float targetScale = _initialWorldScale.x + growthIncrement * _currentStage;
    transform.localScale = Vector3.one * targetScale;

    // 🔄 Hint refresh
    HideSporeHint();
    HideLightHint();
    if (isPlayerNearby)
        TryShowHint();

    // 🌕 Fully grown: spawn light hint once
    if (_currentStage >= maxGrowthStages && !_isFullyGrown)
    {
        _isFullyGrown = true;
        if (lightHintPrefab != null && _lightHintGO == null)
        {
            _lightHintGO = Instantiate(
                lightHintPrefab,
                transform.position + lightHintOffset,
                Quaternion.identity,
                transform
            );
            _lightHintGO.transform.localScale = Vector3.one * lightHintScale;
            HideLightHint();
            if (isPlayerNearby)
                TryShowHint();
        }
    }
}

  void ShowSporeHint()
{
    if (_sporeHintGO != null) return;

    // Find existing child
    Transform existing = transform.Find("SporeIcon");
    if (existing != null)
    {
        _sporeHintGO = existing.gameObject;
        _sporeHintGO.SetActive(true);

        // Match LightHintIcon behavior
        _sporeHintOffset = new Vector3(0f, 0.5f, 0f); // tweak height here
        if (debugLogs)
            Debug.Log($"🌱 Activated existing SporeIcon for {name}");
        return;
    }

    // Fallback in case no child exists
    if (sporeHintPrefab != null)
    {
        _sporeHintOffset = new Vector3(0f, 0.5f, 0f);
        _sporeHintGO = Instantiate(sporeHintPrefab, transform.position + _sporeHintOffset, Quaternion.identity, transform);
        if (debugLogs)
            Debug.Log($"🌱 Spawned fallback SporeIcon prefab for {name}");
    }
}

void HideSporeHint()
{
    if (_sporeHintGO != null)
    {
        _sporeHintGO.SetActive(false);
    }
    if (_activeSporeOwner == this)
        _activeSporeOwner = null;
}

   void ShowLightHint()
{
    // if already exists, just re-enable
    if (_lightHintGO != null)
    {
        _activeLightOwner?.HideLightHint();
        _activeLightOwner = this;
        _lightHintGO.SetActive(true);
        return;
    }

    if (lightHintPrefab == null) return;

    _activeLightOwner?.HideLightHint();
    _activeLightOwner = this;

    // 🌕 Spawn in pure world space (no parent)
    Vector3 worldPos = transform.position + lightHintOffset;
    _lightHintGO = Instantiate(lightHintPrefab, worldPos, Quaternion.identity, null);
    _lightHintGO.transform.localScale = Vector3.one * lightHintScale;
    _lightHintGO.transform.rotation = Quaternion.identity;

    Debug.Log($"🌕 Spawned static light hint for {name} at world {worldPos}");
}


    void HideLightHint()
    {
        if (_lightHintGO != null) _lightHintGO.SetActive(false);
        if (_activeLightOwner == this) _activeLightOwner = null;
    }

    public void GiveLight()
    {
        if (!_isFullyGrown || !isPlayerNearby) return;

        // 1) show the lit‑flower sprite
        if (litFlowerRenderer != null)
            litFlowerRenderer.enabled = true;

        // 2) Award a point only if it hasn't already been awarded for this flower
        if (!hasBeenLit)
        {
            ScoreManager.Instance.AddPoint();
            hasBeenLit = true;
        }

        // 3) **destroy the hint GameObject** so it never re‑appears
        if (_lightHintGO != null)
        {
            Destroy(_lightHintGO);
            _lightHintGO = null;
            if (_activeLightOwner == this)
                _activeLightOwner = null;
        }
    }

    void LateUpdate()
{
    // 🌱 Spore hint — anchored just like light hint
    if (_sporeHintGO != null)
    {
        Vector3 pos = transform.position + _sporeHintOffset;  // <- always relative to flower
        _sporeHintGO.transform.position = pos;
        _sporeHintGO.transform.rotation = Quaternion.identity;
    }

    // 🌕 Light hint — same behavior
    if (_lightHintGO != null)
    {
        Vector3 pos = transform.position + lightHintOffset;
        _lightHintGO.transform.position = pos;
        _lightHintGO.transform.rotation = Quaternion.identity;
    }
}



    void OnTransformParentChanged()
    {
        // if picked up, hide all hints; if dropped and player present, re‑try
        if (isHeld)
        {
            HideSporeHint();
            HideLightHint();
        }
        else if (isPlayerNearby)
        {
            TryShowHint();
        }

        _initialWorldPos = transform.position;
    }

    /// <summary>
    /// External helper to clear any active hints immediately.
    /// </summary>
    public void ClearAllHints()
    {
        HideSporeHint();
        HideLightHint();
    }

   public void BrewFlower()
    {
        Debug.Log($"[BrewFlower] Called on {gameObject.name}, hasBeenLit={hasBeenLit}");

        if (hasBeenLit)
        {
            Debug.Log("[BrewFlower] Point will be removed.");
            ScoreManager.Instance.points = Mathf.Max(0, ScoreManager.Instance.points - 1);
            ScoreManager.Instance.UpdatePointsText();
            hasBeenLit = false;

            if (litFlowerRenderer != null)
                litFlowerRenderer.enabled = false;
        }
        else
        {
            Debug.Log("[BrewFlower] No point removed (hasBeenLit is false).");
        }
    }

}

*/

/* using UnityEngine;
using System.Collections;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld, isPlanted, isPlayerNearby;

    [Header("Growth Settings")]
    public int   maxGrowthStages    = 3;
    public float growthIncrement    = 0.1f;
    public float yPositionIncrement = 0.04f;
    public float maxHeight          = 1.8f;

    [Header("Spore Hint Icon")]
    public GameObject sporeHintPrefab;
    public Vector3[]   hintOffsets;   // length ≥ maxGrowthStages
    public float[]     hintScales;    // length ≥ maxGrowthStages

    [Header("Light Hint Icon")]
    public GameObject lightHintPrefab;
    public Vector3    lightHintOffset;
    public float      lightHintScale  = 1f;

    [Header("Lit Flower Sprite")]
    public SpriteRenderer litFlowerRenderer;

    // Internals
    Vector3   _initialWorldScale;
    Vector3   _initialWorldPos;
    int       _currentStage;
    bool      _isFullyGrown;
    public bool IsFullyGrown => _isFullyGrown;

    GameObject _sporeHintGO;
    Vector3    _sporeHintOffset;
    GameObject _lightHintGO;

    // Only‑one‑hint globals
    static SproutAndLightManager _activeSporeOwner;
    static SproutAndLightManager _activeLightOwner;

    void Awake()
    {
        _initialWorldScale = transform.lossyScale;
        _initialWorldPos   = transform.position;
        _currentStage      = 0;
        _isFullyGrown      = false;

        if (litFlowerRenderer != null)
            litFlowerRenderer.enabled = false;

        HideSporeHint();
        HideLightHint();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNearby = true;
        if (!_isFullyGrown) ShowSporeHint();
        else                ShowLightHint();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNearby = false;
        HideSporeHint();
        HideLightHint();
    }

    public void ResetOnGrowth()
    {
        if (_isFullyGrown) return;
        isPlanted = true;
        _currentStage = Mathf.Min(_currentStage + 1, maxGrowthStages);

        // scale & position snap (world‑space)...
        float targetS = _initialWorldScale.x + growthIncrement * _currentStage;
        Vector3 worldScaleTarget = Vector3.one * targetS;
        float targetY = Mathf.Min(_initialWorldPos.y + yPositionIncrement * _currentStage, maxHeight);
        Vector3 worldPosTarget = new Vector3(_initialWorldPos.x, targetY, _initialWorldPos.z);

        var parent = transform.parent;
        var wRot   = transform.rotation;
        transform.SetParent(null);
        transform.localScale = worldScaleTarget;
        transform.position   = worldPosTarget;
        transform.rotation   = wRot;
        transform.SetParent(parent, true);

        // hints
        HideSporeHint();
        if (isPlayerNearby && _currentStage < maxGrowthStages)
            ShowSporeHint();

        if (_currentStage >= maxGrowthStages)
        {
            _isFullyGrown = true;
            if (lightHintPrefab != null && _lightHintGO == null)
            {
                // **Parent the hint under this flower**
                _lightHintGO = Instantiate(
                    lightHintPrefab,
                    transform.position + lightHintOffset,
                    Quaternion.identity,
                    transform            // <— parent here
                );
                _lightHintGO.transform.localScale = Vector3.one * lightHintScale;
                HideLightHint();
                ShowLightHint();
            }
        }
    }

    void ShowSporeHint()
    {
        if (_sporeHintGO != null || sporeHintPrefab == null) return;
        if (_currentStage >= maxGrowthStages) return;

        _activeSporeOwner?.HideSporeHint();
        _activeSporeOwner = this;

        _sporeHintOffset = (hintOffsets != null && hintOffsets.Length > _currentStage)
            ? hintOffsets[_currentStage]
            : Vector3.up;
        float sc = (hintScales != null && hintScales.Length > _currentStage)
            ? hintScales[_currentStage]
            : 1f;

        _sporeHintGO = Instantiate(sporeHintPrefab);
        _sporeHintGO.transform.localScale = Vector3.one * sc;
    }

    void HideSporeHint()
    {
        if (_sporeHintGO != null) Destroy(_sporeHintGO);
        if (_activeSporeOwner == this) _activeSporeOwner = null;
        _sporeHintGO = null;
    }

    void ShowLightHint()
    {
        if (_lightHintGO == null) return;
        _activeLightOwner?.HideLightHint();
        _activeLightOwner = this;
        _lightHintGO.SetActive(true);
    }

    void HideLightHint()
    {
        if (_lightHintGO != null) _lightHintGO.SetActive(false);
        if (_activeLightOwner == this) _activeLightOwner = null;
    }

    public void GiveLight()
    {
        if (!_isFullyGrown || !isPlayerNearby) return;

        // 1) show the lit‑flower sprite
        if (litFlowerRenderer != null)
            litFlowerRenderer.enabled = true;

        // 2) **destroy the hint GameObject** so it never re‑appears
        if (_lightHintGO != null)
        {
            Destroy(_lightHintGO);
            _lightHintGO = null;
            if (_activeLightOwner == this)
                _activeLightOwner = null;
        }
    }

    void LateUpdate()
    {
        if (_sporeHintGO != null)
        {
            _sporeHintGO.transform.position = transform.position + _sporeHintOffset;
            _sporeHintGO.transform.rotation = Quaternion.identity;
        }
        if (_lightHintGO != null)
        {
            _lightHintGO.transform.position = transform.position + lightHintOffset;
            _lightHintGO.transform.rotation = Quaternion.identity;
        }
    }

    void OnTransformParentChanged()
    {
        _initialWorldPos = transform.position;
    }

    // inside SproutAndLightManager
    public void ClearAllHints()
    {
        HideSporeHint();   // existing private
        HideLightHint();   // existing private
    }

}
*/