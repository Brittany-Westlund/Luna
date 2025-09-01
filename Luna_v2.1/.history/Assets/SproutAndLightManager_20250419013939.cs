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

    [Header("Proximity & Hints")]
    [Tooltip("How close the player must be for hints")]
    public float hintDistance       = 1f;
    public GameObject sporeHintPrefab;
    public Vector3[] hintOffsets;      // length = maxGrowthStages
    public float[]   hintScales;       // length = maxGrowthStages

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
    Transform  _player;

    void Awake()
    {
        // 1) capture your prefab’s starting size/position
        _initialScale    = transform.localScale;
        _initialPosition = transform.position;
        _currentStage    = 0;
        _isFullyGrown    = false;

        // 2) hide any “lit” sprite if you assigned one
        if (litFlowerRenderer != null) litFlowerRenderer.enabled = false;

        // 3) find the player by tag (no trigger needed)
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (_player == null)
            Debug.LogWarning("[Sprout] Could not find a GameObject tagged 'Player'.");

        // 4) ensure no hints start in the scene
        HideSporeHint();
        HideLightHint();
    }

    void Update()
    {
        // distance‑based detection
        if (_player != null)
        {
            float dist = Vector2.Distance(_player.position, transform.position);
            bool nearby = dist <= hintDistance;
            if (nearby != isPlayerNearby)
            {
                isPlayerNearby = nearby;
                Debug.Log($"[Sprout] isPlayerNearby = {nearby} (dist={dist:F2})");

                // show/hide hints appropriately
                if (isPlayerNearby && !_isFullyGrown) ShowSporeHint();
                else                                  HideSporeHint();

                if (isPlayerNearby &&  _isFullyGrown) ShowLightHint();
                else                                  HideLightHint();
            }
        }
    }

    /// <summary>
    /// Called externally (LunaSporeSystem) when you apply a spore.
    /// </summary>
    public void ResetOnGrowth()
    {
        if (_isFullyGrown) return;
        isPlanted = true;

        // bump the stage and animate
        _currentStage = Mathf.Min(_currentStage + 1, maxGrowthStages);
        StopAllCoroutines();
        StartCoroutine(GrowStage());
    }

    IEnumerator GrowStage()
    {
        // calculate start/end transforms
        Vector3 startScale = transform.localScale;
        Vector3 endScale   = _initialScale + Vector3.one * (growthIncrement * _currentStage);

        Vector3 startPos = transform.position;
        float   targetY  = Mathf.Min(
            _initialPosition.y + yPositionIncrement * _currentStage,
            maxHeight
        );
        Vector3 endPos = new Vector3(startPos.x, targetY, startPos.z);

        // smoothly animate
        float elapsed = 0f;
        while (elapsed < stageGrowDuration)
        {
            float t = elapsed / stageGrowDuration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            transform.position   = Vector3.Lerp(startPos,   endPos,   t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // snap to final values
        transform.localScale = endScale;
        transform.position   = endPos;

        // if still growing, show the next hint
        HideSporeHint();
        if (isPlayerNearby && _currentStage < maxGrowthStages)
            ShowSporeHint();

        // once you hit maxGrowthStages, spawn the light‑hint icon
        if (_currentStage >= maxGrowthStages)
        {
            _isFullyGrown = true;
            Debug.Log("[Sprout] Fully grown – spawning light hint.");
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
        // guard: don’t double‑spawn or if no prefab
        if (_sporeHintGO != null || sporeHintPrefab == null) return;
        if (_currentStage >= maxGrowthStages) return;

        Debug.Log($"[Sprout] ShowSporeHint – stage {_currentStage}");

        // pick offset/scale from the arrays (ensure you set them in Inspector!)
        Vector3 offset = hintOffsets.Length > _currentStage
            ? hintOffsets[_currentStage]
            : Vector3.up;
        float   scale  = hintScales.Length  > _currentStage
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
        _sporeHintGO = null;
    }

    void ShowLightHint()
    {
        if (_lightHintGO != null && !_lightHintGO.activeSelf)
        {
            _lightHintGO.SetActive(true);
        }
    }

    void HideLightHint()
    {
        if (_lightHintGO != null && _lightHintGO.activeSelf)
        {
            _lightHintGO.SetActive(false);
        }
    }

    /// <summary>
    /// Call this when Luna “lights” the flower at full growth.
    /// </summary>
    public void GiveLight()
    {
        if (!_isFullyGrown || !isPlayerNearby) return;
        if (litFlowerRenderer != null) litFlowerRenderer.enabled = true;
        HideLightHint();
    }
}
