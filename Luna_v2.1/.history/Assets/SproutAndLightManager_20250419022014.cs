using UnityEngine;

public class SproutAndLightManager : MonoBehaviour
{
    // For other scripts:
    [HideInInspector] public bool isHeld, isPlanted, isPlayerNearby;

    [Header("Growth Settings")]
    [Tooltip("How many spores to full growth")]
    public int   maxGrowthStages   = 3;
    [Tooltip("Exact scale increment per spore")]
    public float growthIncrement   = 0.1f;
    [Tooltip("Y‑offset per spore")]
    public float yPositionIncrement= 0.04f;
    [Tooltip("Max world Y  (to cap height)")]
    public float maxHeight         = 1.8f;

    [Header("Spore Hint")]
    public GameObject sporeHintPrefab;
    [Tooltip("Offset per stage for the spore hint icon")]
    public Vector3[]   hintOffsets;  // length ≥ maxGrowthStages
    [Tooltip("Scale per stage for the spore hint icon")]
    public float[]     hintScales;   // length ≥ maxGrowthStages

    [Header("Light Hint")]
    public GameObject lightHintPrefab;
    public Vector3    lightHintOffset;
    public float      lightHintScale = 1f;

    [Header("Lit Sprite")]
    public SpriteRenderer litFlowerRenderer;

    // Internals
    Vector3   _initialScale;
    Vector3   _initialPos;
    int       _currentStage;
    bool      _isFullyGrown;
    GameObject _sporeHintGO, _lightHintGO;

    // Static so only one icon shows at once
    static SproutAndLightManager _sporeOwner;
    static SproutAndLightManager _lightOwner;

    void Awake()
    {
        _initialScale = transform.localScale;
        _initialPos   = transform.position;
        _currentStage = 0;
        _isFullyGrown = false;

        if (litFlowerRenderer != null)
            litFlowerRenderer.enabled = false;

        HideSporeHint();
        HideLightHint();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        isPlayerNearby = true;
        if (!_isFullyGrown) ShowSporeHint();
        else               ShowLightHint();
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;
        isPlayerNearby = false;
        HideSporeHint();
        HideLightHint();
    }

    /// <summary>
    /// Call this from LunaSporeSystem when a spore is applied.
    /// </summary>
    public void ResetOnGrowth()
    {
        if (_isFullyGrown) return;
        isPlanted = true;

        _currentStage = Mathf.Min(_currentStage + 1, maxGrowthStages);

        // 1) Compute and snap scale
        float newScale = _initialScale.x + growthIncrement * _currentStage;
        transform.localScale = new Vector3(newScale, newScale, _initialScale.z);

        // 2) Compute and snap position
        float newY = Mathf.Min(_initialPos.y + yPositionIncrement * _currentStage, maxHeight);
        transform.position = new Vector3(_initialPos.x, newY, _initialPos.z);

        // 3) Hints
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

        // hide any other
        _sporeOwner?.HideSporeHint();
        _sporeOwner = this;

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
        if (_sporeOwner == this)  _sporeOwner = null;
        _sporeHintGO = null;
    }

    void ShowLightHint()
    {
        if (_lightHintGO == null) return;
        _lightOwner?.HideLightHint();
        _lightOwner = this;
        _lightHintGO.SetActive(true);
    }

    void HideLightHint()
    {
        if (_lightHintGO != null) _lightHintGO.SetActive(false);
        if (_lightOwner == this)  _lightOwner = null;
    }

    /// <summary>
    /// Call when Luna “lights” a fully grown flower.
    /// </summary>
    public void GiveLight()
    {
        if (!_isFullyGrown || !isPlayerNearby) return;
        if (litFlowerRenderer != null)
            litFlowerRenderer.enabled = true;
        HideLightHint();
    }
}
