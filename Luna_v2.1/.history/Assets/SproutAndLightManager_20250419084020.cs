using UnityEngine;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld, isPlanted, isPlayerNearby;

    [Header("Growth Settings")]
    [Tooltip("Number of spores to reach full growth")]
    public int   maxGrowthStages   = 3;
    [Tooltip("World‑space scale added each spore")]
    public float growthIncrement   = 0.1f;
    [Tooltip("Vertical offset (world‑space) per spore")]
    public float yPositionIncrement= 0.04f;
    [Tooltip("Cap on world‑Y position")]
    public float maxHeight         = 1.8f;

    [Header("Spore Hint Icon")]
    public GameObject sporeHintPrefab;
    public Vector3[]   hintOffsets;   // ≥ maxGrowthStages
    public float[]     hintScales;    // ≥ maxGrowthStages

    [Header("Light Hint Icon")]
    public GameObject lightHintPrefab;
    public Vector3    lightHintOffset;
    public float      lightHintScale = 1f;

    [Header("Lit Flower Sprite")]
    public SpriteRenderer litFlowerRenderer;

    // Internals
    private Vector3   _initialWorldScale;
    private Vector3   _initialWorldPos;
    private int       _currentStage;
    private bool      _isFullyGrown;
    private GameObject _sporeHintGO, _lightHintGO;

    // Only‑one‑hint globals
    private static SproutAndLightManager _activeSporeOwner;
    private static SproutAndLightManager _activeLightOwner;

    void Awake()
    {
        // 1) Capture the true world‑scale & world‑position at start
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
        else               ShowLightHint();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNearby = false;
        HideSporeHint();
        HideLightHint();
    }

    /// <summary>
    /// Call when a spore hits this sprout.
    /// </summary>
    public void ResetOnGrowth()
    {
        if (_isFullyGrown) return;
        isPlanted = true;

        // bump stage
        _currentStage = Mathf.Min(_currentStage + 1, maxGrowthStages);

        // Compute the target world‐scale & position instantly
        float targetS = _initialWorldScale.x + growthIncrement * _currentStage;
        Vector3 worldScaleTarget = new Vector3(targetS, targetS, targetS);
        float targetY = Mathf.Min(_initialWorldPos.y + yPositionIncrement * _currentStage, maxHeight);
        Vector3 worldPosTarget = new Vector3(_initialWorldPos.x, targetY, _initialWorldPos.z);

        // ======= Detach, snap world transform, reattach =======
        var parent = transform.parent;
        var wPos    = transform.position;
        var wRot    = transform.rotation;

        // Unparent (worldPositionStays=true by default)
        transform.SetParent(null);

        // Snap scale & position in world-space
        transform.localScale = worldScaleTarget;
        transform.position   = worldPosTarget;
        transform.rotation   = wRot;

        // Reparent back
        transform.SetParent(parent, true);
        // =====================================================

        // Hints
        HideSporeHint();
        if (isPlayerNearby && _currentStage < maxGrowthStages)
            ShowSporeHint();

        if (_currentStage >= maxGrowthStages)
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
                if (isPlayerNearby) ShowLightHint();
                else               HideLightHint();
            }
        }
    }

    void ShowSporeHint()
    {
        if (_sporeHintGO != null || sporeHintPrefab == null) return;
        if (_currentStage >= maxGrowthStages) return;

        // hide other flower’s hint
        _activeSporeOwner?.HideSporeHint();
        _activeSporeOwner = this;

        Vector3 off = (hintOffsets != null && hintOffsets.Length > _currentStage)
            ? hintOffsets[_currentStage]
            : Vector3.up;
        float sc = (hintScales != null && hintScales.Length > _currentStage)
            ? hintScales[_currentStage]
            : 1f;

        _sporeHintGO = Instantiate(
            sporeHintPrefab,
            transform.position + off,
            Quaternion.identity,
            transform
        );
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

    /// <summary>Call when lighting a fully grown flower.</summary>
    public void GiveLight()
    {
        if (!_isFullyGrown || !isPlayerNearby) return;
        if (litFlowerRenderer != null) litFlowerRenderer.enabled = true;
        HideLightHint();
    }
}

//Keep