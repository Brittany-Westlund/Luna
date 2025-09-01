using UnityEngine;
using System.Collections;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld, isPlanted, isPlayerNearby;

    [Header("Growth Settings")]
    public int   maxGrowthStages   = 3;      // number of spores to full growth
    public float growthIncrement   = 0.1f;   // absolute scale added per stage
    public float stageGrowDuration = 0.15f;  // lerp time per stage
    public float yPositionIncrement= 0.04f;  // vertical lift per stage
    public float maxHeight         = 1.8f;   // cap on world‑Y

    [Header("Spore Hint Icon")]
    public GameObject sporeHintPrefab;
    public Vector3[]   hintOffsets; // must be length >= maxGrowthStages
    public float[]     hintScales;  // must be length >= maxGrowthStages

    [Header("Light Hint Icon")]
    public GameObject lightHintPrefab;
    public Vector3    lightHintOffset;
    public float      lightHintScale = 1f;

    [Header("Lit Flower Sprite")]
    public SpriteRenderer litFlowerRenderer;

    // Internals
    Vector3   _initialScale;
    Vector3   _initialPos;
    int       _currentStage;
    bool      _isFullyGrown;
    GameObject _sporeHintGO, _lightHintGO;

    // Static controls so only one hint globally
    static SproutAndLightManager _activeSporeHintOwner;
    static SproutAndLightManager _activeLightHintOwner;

    void Awake()
    {
        // capture the true starting values
        _initialScale = transform.localScale;
        _initialPos   = transform.position;
        _currentStage = 0;
        _isFullyGrown = false;

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

    /// <summary>Called by your spore system to advance one growth stage.</summary>
    public void ResetOnGrowth()
    {
        if (_isFullyGrown) return;
        isPlanted = true;

        _currentStage = Mathf.Min(_currentStage + 1, maxGrowthStages);
        StopAllCoroutines();
        StartCoroutine(GrowStage());
    }

    IEnumerator GrowStage()
    {
        // start→end scale based on initial, never relative
        Vector3 startScale = transform.localScale;
        float   baseX      = _initialScale.x;
        float   maxX       = baseX + growthIncrement * maxGrowthStages;
        float   targetX    = Mathf.Min(baseX + growthIncrement * _currentStage, maxX);
        Vector3 endScale   = new Vector3(targetX, targetX, startScale.z);

        // start→end position
        Vector3 startPos = transform.position;
        float   targetY  = Mathf.Min(_initialPos.y + yPositionIncrement * _currentStage, maxHeight);
        Vector3 endPos   = new Vector3(startPos.x, targetY, startPos.z);

        float elapsed = 0f;
        while (elapsed < stageGrowDuration)
        {
            float t = elapsed / stageGrowDuration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            transform.position   = Vector3.Lerp(startPos,   endPos,   t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // finalize
        transform.localScale = endScale;
        transform.position   = endPos;

        // next spore hint if still not maxed
        HideSporeHint();
        if (isPlayerNearby && _currentStage < maxGrowthStages)
            ShowSporeHint();

        // if we just hit full growth, spawn light hint
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

        // hide any other flower's hint
        _activeSporeHintOwner?.HideSporeHint();
        _activeSporeHintOwner = this;

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
        if (_activeSporeHintOwner == this) _activeSporeHintOwner = null;
        _sporeHintGO = null;
    }

    void ShowLightHint()
    {
        if (_lightHintGO == null) return;

        // hide other flower's light hint
        _activeLightHintOwner?.HideLightHint();
        _activeLightHintOwner = this;
        _lightHintGO.SetActive(true);
    }

    void HideLightHint()
    {
        if (_lightHintGO != null)
            _lightHintGO.SetActive(false);
        if (_activeLightHintOwner == this)
            _activeLightHintOwner = null;
    }

    /// <summary>Call when Luna “lights” the fully grown flower.</summary>
    public void GiveLight()
    {
        if (!_isFullyGrown || !isPlayerNearby) return;
        if (litFlowerRenderer != null)
            litFlowerRenderer.enabled = true;
        HideLightHint();
    }
}
