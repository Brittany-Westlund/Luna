using UnityEngine;
using System.Collections;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld, isPlanted, isPlayerNearby;

    [Header("Growth Settings")]
    public int   maxGrowthStages   = 3;
    public float growthIncrement   = 0.1f;
    public float stageGrowDuration = 0.15f;
    public float yPositionIncrement= 0.04f;
    public float maxHeight         = 1.8f;

    [Header("Spore Hint Icon")]
    public GameObject sporeHintPrefab;
    public Vector3[]   hintOffsets;    // length = maxGrowthStages
    public float[]     hintScales;     // length = maxGrowthStages

    [Header("Light Hint Icon")]
    public GameObject lightHintPrefab;
    public Vector3    lightHintOffset;
    public float      lightHintScale = 1f;

    [Header("Lit Flower Sprite")]
    public SpriteRenderer litFlowerRenderer;

    // Internals
    private Vector3   _initialScale;
    private Vector3   _initialPosition;
    private int       _currentStage;
    private bool      _isFullyGrown;
    private GameObject _sporeHintGO, _lightHintGO;

    // Keep only one icon active globally
    private static SproutAndLightManager _currentSporeOwner;
    private static SproutAndLightManager _currentLightOwner;

    void Awake()
    {
        // Capture starting scale/position
        _initialScale    = transform.localScale;
        _initialPosition = transform.position;
        _currentStage    = 0;
        _isFullyGrown    = false;

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

    /// <summary>Call this when a spore is applied.</summary>
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

        // Add a fixed increment to the current scale, capped at initial + (increment×stages)
        float baseX  = _initialScale.x;
        float maxX   = baseX + growthIncrement * maxGrowthStages;
        float desiredX = startScale.x + growthIncrement;
        float targetX  = Mathf.Min(desiredX, maxX);

        Vector3 endScale = new Vector3(targetX, targetX, startScale.z);

        Vector3 startPos = transform.position;
        float   targetY  = Mathf.Min(
            _initialPosition.y + yPositionIncrement * _currentStage,
            maxHeight
        );
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

        // Snap to final
        transform.localScale = endScale;
        transform.position   = endPos;

        // Show next spore hint if still growing
        HideSporeHint();
        if (isPlayerNearby && _currentStage < maxGrowthStages)
            ShowSporeHint();

        // If we've reached max stage, spawn the light hint
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

        if (_currentSporeOwner != null && _currentSporeOwner != this)
            _currentSporeOwner.HideSporeHint();
        _currentSporeOwner = this;

        Vector3 offset = (hintOffsets != null && hintOffsets.Length > _currentStage)
            ? hintOffsets[_currentStage]
            : Vector3.up;
        float scale = (hintScales != null && hintScales.Length > _currentStage)
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
        if (_lightHintGO != null)
            _lightHintGO.SetActive(false);
        if (_currentLightOwner == this)
            _currentLightOwner = null;
    }

    /// <summary>Call this when you “light” a fully grown flower.</summary>
    public void GiveLight()
    {
        if (!_isFullyGrown || !isPlayerNearby) return;
        if (litFlowerRenderer != null)
            litFlowerRenderer.enabled = true;
        HideLightHint();
    }
}
