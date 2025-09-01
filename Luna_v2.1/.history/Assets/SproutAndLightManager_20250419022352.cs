using UnityEngine;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld, isPlanted, isPlayerNearby;

    [Header("Growth Settings")]
    [Tooltip("How many spores to full growth")]
    public int   maxGrowthStages   = 3;
    [Tooltip("World‐scale increment per spore")]
    public float growthIncrement   = 0.1f;
    [Tooltip("Y‐offset per spore")]
    public float yPositionIncrement= 0.04f;
    [Tooltip("Max world Y (cap height)")]
    public float maxHeight         = 1.8f;

    [Header("Spore Hint Icon")]
    public GameObject sporeHintPrefab;
    public Vector3[]   hintOffsets;   // length ≥ maxGrowthStages
    public float[]     hintScales;    // length ≥ maxGrowthStages

    [Header("Light Hint Icon")]
    public GameObject lightHintPrefab;
    public Vector3    lightHintOffset;
    public float      lightHintScale = 1f;

    [Header("Lit Flower Sprite")]
    public SpriteRenderer litFlowerRenderer;

    // internals
    private Vector3   _initialWorldScale;
    private Vector3   _initialPos;
    private int       _currentStage;
    private bool      _isFullyGrown;
    private GameObject _sporeHintGO, _lightHintGO;

    // only‑one‑hint logic
    private static SproutAndLightManager _activeSporeOwner;
    private static SproutAndLightManager _activeLightOwner;

    void Awake()
    {
        // capture world scale once
        _initialWorldScale = transform.lossyScale;
        _initialPos        = transform.position;
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

    /// <summary>Call when a spore is applied.</summary>
    public void ResetOnGrowth()
    {
        if (_isFullyGrown) return;
        isPlanted = true;
        _currentStage = Mathf.Min(_currentStage + 1, maxGrowthStages);

        // 1) Compute new world scale
        float targetScaleValue = _initialWorldScale.x + growthIncrement * _currentStage;
        Vector3 targetWorldScale = new Vector3(
            targetScaleValue,
            targetScaleValue,
            targetScaleValue
        );

        // 2) Apply it _in world space_ via localScale
        SetWorldScale(targetWorldScale);

        // 3) Snap position
        float newY = Mathf.Min(
            _initialPos.y + yPositionIncrement * _currentStage,
            maxHeight
        );
        transform.position = new Vector3(_initialPos.x, newY, _initialPos.z);

        // 4) Hints
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

    // Converts a desired world scale into localScale given current parent hierarchy
    void SetWorldScale(Vector3 worldScale)
    {
        if (transform.parent == null)
        {
            transform.localScale = worldScale;
        }
        else
        {
            Vector3 parentWorld = transform.parent.lossyScale;
            transform.localScale = new Vector3(
                worldScale.x / parentWorld.x,
                worldScale.y / parentWorld.y,
                worldScale.z / parentWorld.z
            );
        }
    }

    void ShowSporeHint()
    {
        if (_sporeHintGO != null || sporeHintPrefab == null) return;
        if (_currentStage >= maxGrowthStages) return;

        _activeSporeOwner?.HideSporeHint();
        _activeSporeOwner = this;

        Vector3 offset = (hintOffsets.Length > _currentStage)
            ? hintOffsets[_currentStage]
            : Vector3.up;
        float scale = (hintScales.Length > _currentStage)
            ? hintScales[_currentStage]
            : 1f;

        _sporeHintGO = Instantiate(
            sporeHintPrefab,
            transform.position + offset,
            Quaternion.identity,
            transform
        );
        _sporeHintGO.transform.localScale = Vector3.one * scale;
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
        if (litFlowerRenderer != null)
            litFlowerRenderer.enabled = true;
        HideLightHint();
    }
}
