using UnityEngine;
using System.Collections;

public class SproutAndLightManager : MonoBehaviour
{
    // Compatibility fields for other scripts
    [HideInInspector] public bool isHeld    = false;
    [HideInInspector] public bool isPlanted = false;
    [HideInInspector] public bool isPlayerNearby = false;

    [Header("Growth Settings")]
    [Tooltip("How much to increase localScale each stage")]
    public float growthIncrement     = 0.1f;
    [Tooltip("How much to raise the Y position each stage")]
    public float yPositionIncrement  = 0.04f;
    [Tooltip("Maximum localScale")]
    public float maxScale            = 0.3f;
    [Tooltip("Maximum world Y position")]
    public float maxHeight           = 1.8f;
    [Tooltip("Number of spore applications until full growth")]
    public int   maxGrowthStages     = 3;
    [Tooltip("Duration of each stage’s smooth grow")]
    public float stageGrowDuration   = 0.15f;

    [Header("Spore Hint Icon")]
    public GameObject sporeHintPrefab;
    public Vector3    hintOffsetStage1  = new Vector3(0,1.5f,0);
    public Vector3    hintOffsetStage2  = new Vector3(0,1.1f,0);
    public Vector3    hintOffsetStage3  = new Vector3(0,0.85f,0);
    public float      hintScaleStage1   = 1f;
    public float      hintScaleStage2   = 0.75f;
    public float      hintScaleStage3   = 0.6f;

    [Header("Light Hint Icon")]
    public GameObject lightHintPrefab;
    public Vector3    lightHintOffset   = new Vector3(0,0.2f,0);
    public float      lightHintScale    = 0.6f;

    [Header("Lit Flower Sprite")]
    public SpriteRenderer litFlowerRenderer; // assign your LitFlowerB here

    // Internal state
    int      _currentStage    = 0;
    bool     _isFullyGrown    = false;
    bool     _sporeHintActive = false;
    bool     _lightHintActive = false;
    GameObject _sporeHintGO, _lightHintGO;

    void Awake()
    {
        // Initialize to first, small stage
        transform.localScale = Vector3.one * growthIncrement;
        _currentStage = 0;
        _isFullyGrown = false;

        // Disable lights and hints
        if (litFlowerRenderer != null) litFlowerRenderer.enabled = false;
        HideSporeHint();
        HideLightHint();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (!_isFullyGrown) ShowSporeHint();
            else               ShowLightHint();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            HideSporeHint();
            HideLightHint();
        }
    }

    /// <summary>
    /// Called by LunaSporeSystem when Luna applies a spore to grow the flower.
    /// </summary>
    public void ResetOnGrowth()
    {
        if (_isFullyGrown) return;
        isPlanted = true;  // now officially planted

        _currentStage = Mathf.Min(_currentStage + 1, maxGrowthStages);
        StopAllCoroutines();
        StartCoroutine(GrowStage());
    }

    IEnumerator GrowStage()
    {
        Vector3 startScale = transform.localScale;
        float targetScaleValue = Mathf.Min(startScale.x + growthIncrement, maxScale);
        Vector3 endScale = Vector3.one * targetScaleValue;

        Vector3 startPos = transform.position;
        float targetY = Mathf.Min(transform.position.y + yPositionIncrement, maxHeight);
        Vector3 endPos = new Vector3(transform.position.x, targetY, transform.position.z);

        float elapsed = 0f;
        while (elapsed < stageGrowDuration)
        {
            float t = elapsed / stageGrowDuration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            transform.position   = Vector3.Lerp(startPos,   endPos,   t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final values
        transform.localScale = endScale;
        transform.position   = endPos;

        // Update hints
        HideSporeHint();
        if (isPlayerNearby && _currentStage < maxGrowthStages)
            ShowSporeHint();

        if (_currentStage >= maxGrowthStages)
        {
            _isFullyGrown = true;
            // Instantiate but hide until player approaches
            if (lightHintPrefab != null)
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
        if (_sporeHintActive || sporeHintPrefab == null || _currentStage >= maxGrowthStages) return;

        Vector3 offset;
        float   scale;
        switch (_currentStage)
        {
            case 0:
                offset = hintOffsetStage1; scale = hintScaleStage1; break;
            case 1:
                offset = hintOffsetStage2; scale = hintScaleStage2; break;
            default:
                offset = hintOffsetStage3; scale = hintScaleStage3; break;
        }

        _sporeHintGO = Instantiate(
            sporeHintPrefab,
            transform.position + offset,
            Quaternion.identity,
            transform
        );
        _sporeHintGO.transform.localScale = Vector3.one * scale;
        _sporeHintActive = true;
    }

    void HideSporeHint()
    {
        if (_sporeHintGO != null) Destroy(_sporeHintGO);
        _sporeHintGO     = null;
        _sporeHintActive = false;
    }

    void ShowLightHint()
    {
        if (_lightHintGO == null || _lightHintActive) return;
        _lightHintGO.SetActive(true);
        _lightHintActive = true;
    }

    void HideLightHint()
    {
        if (_lightHintGO != null) _lightHintGO.SetActive(false);
        _lightHintActive = false;
    }

    /// <summary>
    /// Call this when Luna “lights” the flower at full growth.
    /// </summary>
    public void GiveLight()
    {
        if (!_isFullyGrown || !isPlayerNearby) return;

        if (litFlowerRenderer != null)
            litFlowerRenderer.enabled = true;

        HideLightHint();
    }
}
