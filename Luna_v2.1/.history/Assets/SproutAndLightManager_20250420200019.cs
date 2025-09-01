using UnityEngine;
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
}


/* using UnityEngine;
using System.Collections;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld, isPlanted, isPlayerNearby;

    [Header("Growth Settings")]
    public int   maxGrowthStages   = 3;
    public float growthIncrement   = 0.1f;
    public float yPositionIncrement= 0.04f;
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

    // Internals
    Vector3   _initialWorldScale;
    Vector3   _initialWorldPos;
    int       _currentStage;
    bool      _isFullyGrown;

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
        else               ShowLightHint();
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

        // scale & position snap (world‑space)
        float targetS = _initialWorldScale.x + growthIncrement * _currentStage;
        Vector3 worldScaleTarget = Vector3.one * targetS;

        float targetY = Mathf.Min(_initialWorldPos.y + yPositionIncrement * _currentStage, maxHeight);
        Vector3 worldPosTarget = new Vector3(_initialWorldPos.x, targetY, _initialWorldPos.z);

        // detach → snap → reattach
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
                // unparented instance
                _lightHintGO = Instantiate(lightHintPrefab);
                _lightHintGO.transform.localScale = Vector3.one * lightHintScale;
                HideLightHint();
                // position immediately
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

        // pick offset & scale
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
        if (litFlowerRenderer != null) litFlowerRenderer.enabled = true;
        HideLightHint();
    }

    void LateUpdate()
    {
        // **manually** follow the flower's position (no rotation)
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

    /// <summary>
    /// Whenever this sprout gets re‑parented (e.g. picked up or planted),
    /// update its saved “home” so ResetOnGrowth() will grow in place.
    /// </summary>
    void OnTransformParentChanged()
    {
        _initialWorldPos = transform.position;
    }

}
*/