using UnityEngine;
using System.Collections;

public class SproutAndLightManager : MonoBehaviour
{
    // <<< Re‑added for your other scripts to compile >>>
    [HideInInspector] public bool isHeld    = false;
    [HideInInspector] public bool isPlanted = false;

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
    public SpriteRenderer litFlowerRenderer; // assign your LitFlowerB here

    // internals
    int    _currentStage    = 0;
    bool   _isFullyGrown    = false;
    bool   _sporeHintActive = false;
    bool   _lightHintActive = false;
    bool   _isPlayerNearby  = false;
    GameObject _sporeHintGO, _lightHintGO;

    void Awake()
    {
        // start tiny
        transform.localScale = Vector3.one * growthIncrement;
        _currentStage = 0;
        _isFullyGrown = false;

        // hide everything
        if (litFlowerRenderer != null) litFlowerRenderer.enabled = false;
        HideSporeHint();
        HideLightHint();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerNearby = true;
            if (!_isFullyGrown) ShowSporeHint();
            else               ShowLightHint();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _isPlayerNearby = false;
            HideSporeHint();
            HideLightHint();
        }
    }

    /// <summary>
    /// Called by your spore system when Luna applies a spore to grow the flower.
    /// </summary>
    public void ResetOnGrowth()
    {
        if (_isFullyGrown) return;
        isPlanted = true;            // now it’s planted in a plot (for GardenSpot checks)

        _currentStage++;
        StopAllCoroutines();
        StartCoroutine(GrowStage());
    }

    IEnumerator GrowStage()
    {
        // compute start/end
        Vector3 startScale = transform.localScale;
        Vector3 endScale   = Vector3.one
                           * Mathf.Min(startScale.x + growthIncrement, maxScale);
        Vector3 startPos   = transform.position;
        Vector3 endPos     = new Vector3(
                              transform.position.x,
                              Mathf.Min(transform.position.y + yPositionIncrement, maxHeight),
                              transform.position.z
                            );

        // animate
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

        // cleanup old hint, show new one if still growing
        HideSporeHint();
        if (_isPlayerNearby && _currentStage < maxGrowthStages)
            ShowSporeHint();

        // if we just hit the max stage, mark fully grown and spawn the light hint
        if (_currentStage >= maxGrowthStages)
        {
            _isFullyGrown = true;

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
        if (_sporeHintActive || sporeHintPrefab == null) return;
        if (_currentStage >= maxGrowthStages) return;

        Vector3 offset;
        float   scale;
        if (_currentStage == 0)
        {
            offset = hintOffsetStage1; scale = hintScaleStage1;
        }
        else if (_currentStage == 1)
        {
            offset = hintOffsetStage2; scale = hintScaleStage2;
        }
        else
        {
            offset = hintOffsetStage3; scale = hintScaleStage3;
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
        _sporeHintGO      = null;
        _sporeHintActive  = false;
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
        if (!_isFullyGrown || !_isPlayerNearby) return;
        isHeld = false; // just in case

        if (litFlowerRenderer != null)
            litFlowerRenderer.enabled = true;

        HideLightHint();
    }
}
