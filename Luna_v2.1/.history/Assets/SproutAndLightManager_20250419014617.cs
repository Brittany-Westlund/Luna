using UnityEngine;
using System.Collections;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld, isPlanted, isPlayerNearby;

    [Header("Growth Settings")]
    public int   maxGrowthStages   = 3;
    public float growthIncrement   = 0.1f;      // percent per stage
    public float stageGrowDuration = 0.15f;
    public float yPositionIncrement= 0.04f;
    public float maxHeight         = 1.8f;

    [Header("Spore Hint Icon")]
    public GameObject sporeHintPrefab;
    public Vector3[]   hintOffsets;    // must match maxGrowthStages
    public float[]     hintScales;     

    [Header("Light Hint Icon")]
    public GameObject lightHintPrefab;
    public Vector3    lightHintOffset;
    public float      lightHintScale = 1f;

    [Header("Lit Flower Sprite")]
    public SpriteRenderer litFlowerRenderer;

    // Internals
    Vector3   _initialScale;
    Vector3   _initialPosition;
    int       _currentStage;
    bool      _isFullyGrown;
    GameObject _sporeHintGO, _lightHintGO;

    // static so only one icon shows at a time
    static SproutAndLightManager _currentSporeOwner;
    static SproutAndLightManager _currentLightOwner;

    void Awake()
    {
        _initialScale    = transform.localScale;
        _initialPosition = transform.position;
        _currentStage    = 0;
        _isFullyGrown    = false;

        if (litFlowerRenderer != null) litFlowerRenderer.enabled = false;
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

    /// <summary>Called by your spore system to grow to the next stage.</summary>
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
        Vector3 startScale = transform.localScale;

        // ◀️ RELATIVE SCALING HERE ▶️
        float factor = 1f + growthIncrement * _currentStage;
        Vector3 endScale = new Vector3(
            _initialScale.x * factor,
            _initialScale.y * factor,
            _initialScale.z * factor
        );

        Vector3 startPos = transform.position;
        float   targetY  = Mathf.Min(
            _initialPosition.y + yPositionIncrement * _currentStage,
            maxHeight
        );
        Vector3 endPos = new Vector3(startPos.x, targetY, startPos.z);

        float elapsed = 0f;
        while (elapsed < stageGrowDuration)
        {
            float t = elapsed / stageGrowDuration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            transform.position   = Vector3.Lerp(startPos,   endPos,   t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = endScale;
        transform.position   = endPos;

        // next spore hint if still growing
        HideSporeHint();
        if (isPlayerNearby && _currentStage < maxGrowthStages)
            ShowSporeHint();

        // fully grown → create (but hide) the light hint
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
                HideLightHint();
            }
        }
    }

    void ShowSporeHint()
    {
        if (_sporeHintGO != null || sporeHintPrefab == null) return;
        if (_currentStage >= maxGrowthStages) return;

        // hide the last flower's icon
        if (_currentSporeOwner != null && _currentSporeOwner != this)
            _currentSporeOwner.HideSporeHint();
        _currentSporeOwner = this;

        Vector3 offset = hintOffsets.Length > _currentStage
            ? hintOffsets[_currentStage]
            : Vector3.up;
        float scale = hintScales.Length > _currentStage
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
        if (_currentSporeOwner == this) _currentSporeOwner = null;
        _sporeHintGO = null;
    }

    void ShowLightHint()
    {
        if (_lightHintGO == null) return;
        if (_currentLightOwner != null && _currentLightOwner != this)
            _currentLightOwner.HideLightHint();
        _currentLightOwner = this;
        _lightHintGO.SetActive(true);
    }

    void HideLightHint()
    {
        if (_lightHintGO != null) _lightHintGO.SetActive(false);
        if (_currentLightOwner == this) _currentLightOwner = null;
    }

    /// <summary>Called by your spore system when you light a fully grown flower.</summary>
    public void GiveLight()
    {
        if (!_isFullyGrown || !isPlayerNearby) return;
        if (litFlowerRenderer != null) litFlowerRenderer.enabled = true;
        HideLightHint();
    }
}
