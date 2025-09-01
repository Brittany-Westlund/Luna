using UnityEngine;
using System.Collections;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld    = false;
    [HideInInspector] public bool isPlanted = false;
    [HideInInspector] public bool isPlayerNearby = false;

    [Header("Growth Settings")]
    public float growthIncrement     = 0.1f;
    public float yPositionIncrement  = 0.04f;
    public float maxScale            = 0.3f;
    public float maxHeight           = 1.8f;
    public int   maxGrowthStages     = 3;
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
    public SpriteRenderer litFlowerRenderer;

    // Internal state
    int      _currentStage    = 0;
    bool     _isFullyGrown    = false;
    bool     _sporeHintActive = false;
    bool     _lightHintActive = false;
    GameObject _sporeHintGO, _lightHintGO;

    void Awake()
    {
        transform.localScale = Vector3.one * growthIncrement;
        _currentStage = 0;
        _isFullyGrown = false;

        if (litFlowerRenderer != null) litFlowerRenderer.enabled = false;
        HideSporeHint();
        HideLightHint();
    }

    void Update()
    {
        // *** Force‑show spore hint in Update if conditions met ***
        if (isPlayerNearby && !_isFullyGrown && _currentStage < maxGrowthStages && !_sporeHintActive)
        {
            Debug.Log($"[Sprout][Update] Spore hint condi met at stage {_currentStage}");
            ShowSporeHint();
        }

        // (Rest of Update left empty; growth only driven by ResetOnGrowth)
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log("[Sprout] OnTriggerEnter2D: Player nearby");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            Debug.Log("[Sprout] OnTriggerExit2D: Player left");
            HideSporeHint();
            HideLightHint();
        }
    }

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
        float   targetX    = Mathf.Min(startScale.x + growthIncrement, maxScale);
        Vector3 endScale   = new Vector3(targetX, targetX, startScale.z);
        Vector3 startPos   = transform.position;
        float   targetY    = Mathf.Min(startPos.y + yPositionIncrement, maxHeight);
        Vector3 endPos     = new Vector3(startPos.x, targetY, startPos.z);

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

        HideSporeHint();
        if (isPlayerNearby && _currentStage < maxGrowthStages)
            ShowSporeHint();

        if (_currentStage >= maxGrowthStages)
        {
            _isFullyGrown = true;
            Debug.Log("[Sprout] Fully grown! Spawning light hint.");
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

        Debug.Log($"[Sprout] ShowSporeHint() at stage {_currentStage}");

        Vector3 offset;
        float scale;
        switch (_currentStage)
        {
            case 0: offset = hintOffsetStage1; scale = hintScaleStage1; break;
            case 1: offset = hintOffsetStage2; scale = hintScaleStage2; break;
            default:offset = hintOffsetStage3; scale = hintScaleStage3; break;
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

    public void GiveLight()
    {
        if (!_isFullyGrown || !isPlayerNearby) return;
        if (litFlowerRenderer != null) litFlowerRenderer.enabled = true;
        HideLightHint();
    }
}
