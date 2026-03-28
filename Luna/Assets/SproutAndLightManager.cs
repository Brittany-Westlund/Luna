using UnityEngine;
using System.Collections;

public class SproutAndLightManager : MonoBehaviour
{
    [HideInInspector] public bool isHeld;
    [HideInInspector] public bool isPlanted;
    [HideInInspector] public bool isPlayerNearby;

    [Header("Growth Settings")]
    public int maxGrowthStages = 3;
    public float growthScalePerStage = 0.15f;
    public float growthRisePerStage = 0.08f;

    [Header("Growth Target")]
    [Tooltip("Only this transform will scale/rise as the flower grows. Do NOT use the flower root.")]
    public Transform growthTarget;

    [Header("Hint Anchor")]
    [Tooltip("Optional anchor for hint positioning. Defaults to the flower root.")]
    public Transform hintAnchor;

    [Header("Spore Hint Icon (prefers child)")]
    public GameObject sporeHintPrefab;
    public string sporeChildName = "SporePrompt";
    public Vector3 sporeHintOffset = new Vector3(0f, 0.5f, 0f);
    public float sporeHintScale = 1f;

    [Header("Light Hint Icon")]
    public GameObject lightHintPrefab;
    public Vector3 lightHintOffset = new Vector3(0f, 0.5f, 0f);
    public float lightHintScale = 1f;

    [Header("Hint Rise While Growing")]
    [Tooltip("Extra upward motion for hints as the flower grows, without scaling the hints themselves.")]
    public float hintRisePerStage = 0.08f;

    [Header("Player Prompt Detection")]
    [Tooltip("Optional dedicated player prompt trigger. If left empty, the script will try to find a child named PlayerHead under the Player.")]
    public Transform playerHeadOverride;
    [Tooltip("Child name searched under the Player object when playerHeadOverride is not assigned.")]
    public string playerHeadName = "PlayerHead";
    [Tooltip("Fallback distance check from the PlayerHead/root to this flower if touching is not detected.")]
    public float playerPromptDistance = 0.5f;

    [Header("Lit Flower Sprite")]
    public SpriteRenderer litFlowerRenderer;

    [Header("Debug")]
    public bool debugLogs = false;

    private Vector3 _initialWorldPos;
    private int _currentStage;
    private bool _isFullyGrown;
    public bool IsFullyGrown => _isFullyGrown;

    private bool hasBeenLit = false;
    private bool hasBeenLitAlreadyCounted = false;

    private GameObject _sporeHintGO;
    private GameObject _lightHintGO;

    private Vector3 _growthBaseLocalScale = Vector3.one;
    private Vector3 _growthBaseLocalPosition = Vector3.zero;

    private Vector3 _sporeHintBaseLocalScale = Vector3.one;
    private Vector3 _lightHintBaseLocalScale = Vector3.one;

    private const float GARDEN_CHECK_RADIUS = 0.1f;
    private const float MIN_SCALE_EPSILON = 0.0001f;

    private Transform _cachedPlayerRoot;
    private Transform _cachedPlayerHead;
    private Collider2D _cachedFlowerCollider;
    private Collider2D _cachedPlayerHeadCollider;

    private void Awake()
    {
        if (growthTarget == null)
            growthTarget = transform;

        if (hintAnchor == null)
            hintAnchor = transform;

        _initialWorldPos = transform.position;
        _currentStage = 0;
        _isFullyGrown = false;

        _growthBaseLocalScale = growthTarget.localScale;
        _growthBaseLocalPosition = growthTarget.localPosition;

        if (litFlowerRenderer != null)
            litFlowerRenderer.enabled = false;

        _cachedFlowerCollider = GetComponent<Collider2D>();

        CacheSporeHintReference();
        CacheLightHintReference();
        CachePlayerPromptReferences();
    }

    private void OnEnable()
    {
        CachePlayerPromptReferences();
    }

    private void Update()
    {
        isPlayerNearby = IsPlayerPromptCloseEnough();
        TryShowHint();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsPromptCollider(other))
            return;

        isPlayerNearby = true;
        TryShowHint(force: true);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!IsPromptCollider(other))
            return;

        isPlayerNearby = true;
        TryShowHint(force: true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPromptCollider(other))
            return;

        isPlayerNearby = false;
        HideSporeHint();
        HideLightHint();
    }

    private bool IsPromptCollider(Collider2D other)
    {
        if (other == null)
            return false;

        CachePlayerPromptReferences();

        if (_cachedPlayerHeadCollider != null)
            return other == _cachedPlayerHeadCollider;

        return other.CompareTag("Player");
    }

    private bool IsInOrNearGarden()
    {
        for (Transform t = transform; t != null; t = t.parent)
        {
            if (t.CompareTag("Garden"))
                return true;
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, GARDEN_CHECK_RADIUS);
        foreach (Collider2D c in hits)
        {
            if (c.CompareTag("Garden"))
                return true;
        }

        return false;
    }

    private void TryShowHint(bool force = false)
    {
        if (!isPlayerNearby || isHeld || !IsInOrNearGarden())
        {
            HideSporeHint();
            HideLightHint();
            return;
        }

        if (!_isFullyGrown)
            ShowSporeHint(force);
        else if (!hasBeenLit)
            ShowLightHint(force);
        else
            HideLightHint();
    }

    public void ResetOnGrowth()
    {
        if (!IsInOrNearGarden())
        {
            if (debugLogs)
                Debug.Log($"[SproutAndLightManager] {name} cannot grow outside of a garden.");
            return;
        }

        if (_isFullyGrown)
        {
            if (debugLogs)
                Debug.Log($"[SproutAndLightManager] {name} is already fully grown.");
            return;
        }

        isPlanted = true;

        _currentStage = Mathf.Min(_currentStage + 1, maxGrowthStages);
        ApplyGrowthVisuals();

        if (_currentStage >= maxGrowthStages)
        {
            _isFullyGrown = true;
            HideSporeHint();

            if (!hasBeenLit && isPlayerNearby)
                ShowLightHint(true);
        }
        else
        {
            StartCoroutine(ShowSporeHintNextFrame());

            if (isPlayerNearby)
                ShowSporeHint(true);
        }

        if (debugLogs)
        {
            Debug.Log($"[SproutAndLightManager] {name} grew to stage {_currentStage}/{maxGrowthStages}. FullyGrown={_isFullyGrown}");
        }
    }

    private void ApplyGrowthVisuals()
    {
        if (growthTarget == null)
            return;

        float scaleMultiplier = 1f + (growthScalePerStage * _currentStage);
        float riseAmount = growthRisePerStage * _currentStage;

        growthTarget.localScale = _growthBaseLocalScale * scaleMultiplier;
        growthTarget.localPosition = _growthBaseLocalPosition + new Vector3(0f, riseAmount, 0f);
    }

    private IEnumerator ShowSporeHintNextFrame()
    {
        yield return new WaitForFixedUpdate();

        if (_isFullyGrown || isHeld)
            yield break;

        if (IsPlayerPromptCloseEnough())
        {
            ShowSporeHint(true);

            if (debugLogs)
                Debug.Log($"🌱 Forced spore hint shown for {name} right after planting.");
        }
    }

    public void ForceShowSporeHint()
    {
        if (_isFullyGrown || isHeld)
            return;

        if (IsPlayerPromptCloseEnough())
        {
            ShowSporeHint(true);

            if (debugLogs)
                Debug.Log($"🌱 Force showing spore hint for {name} because PlayerHead is close.");
        }
    }

    private bool IsPlayerPromptCloseEnough()
    {
        CachePlayerPromptReferences();

        Transform promptTarget = _cachedPlayerHead != null ? _cachedPlayerHead : _cachedPlayerRoot;
        if (promptTarget == null)
            return false;

        if (_cachedFlowerCollider == null)
            _cachedFlowerCollider = GetComponent<Collider2D>();

        if (_cachedFlowerCollider != null && _cachedPlayerHeadCollider != null)
        {
            if (_cachedFlowerCollider.IsTouching(_cachedPlayerHeadCollider))
                return true;
        }

        float distance = Vector2.Distance(transform.position, promptTarget.position);
        return distance < playerPromptDistance;
    }

    private void ShowSporeHint(bool force = false)
    {
        if (_currentStage >= maxGrowthStages || !IsInOrNearGarden())
        {
            HideSporeHint();
            return;
        }

        if (_sporeHintGO == null)
            CacheSporeHintReference();

        if (_sporeHintGO == null && sporeHintPrefab != null)
        {
            _sporeHintGO = Instantiate(sporeHintPrefab, transform);
            _sporeHintBaseLocalScale = _sporeHintGO.transform.localScale;
        }

        if (_sporeHintGO != null)
        {
            _sporeHintGO.SetActive(true);
            _sporeHintGO.transform.localPosition = sporeHintOffset;

            if (debugLogs && force)
                Debug.Log($"🌱 Spore hint active for {name}");
        }
    }

    private void HideSporeHint()
    {
        if (_sporeHintGO != null)
            _sporeHintGO.SetActive(false);
    }

    private void ShowLightHint(bool force = false)
    {
        if (!_isFullyGrown || !IsInOrNearGarden() || hasBeenLit)
            return;

        if (_lightHintGO == null)
            CacheLightHintReference();

        if (_lightHintGO == null && lightHintPrefab != null)
        {
            _lightHintGO = Instantiate(lightHintPrefab, transform);
            _lightHintBaseLocalScale = _lightHintGO.transform.localScale;
            _lightHintGO.transform.localPosition = lightHintOffset;
        }

        if (_lightHintGO != null)
        {
            _lightHintGO.SetActive(true);

            if (debugLogs && force)
                Debug.Log($"🌕 Light hint active for {name}");
        }
    }

    public void HideLightHint()
    {
        if (_lightHintGO != null)
            _lightHintGO.SetActive(false);
    }

    public void GiveLight()
    {
        if (!_isFullyGrown || !isPlayerNearby || !IsInOrNearGarden())
            return;

        ApplyLitState("GiveLight");
    }

    public void ForceGiveLightFromFairyfly()
    {
        if (!_isFullyGrown || !IsInOrNearGarden())
            return;

        ApplyLitState("ForceGiveLightFromFairyfly");
    }

    private void ApplyLitState(string source)
    {
        if (litFlowerRenderer != null)
        {
            litFlowerRenderer.gameObject.SetActive(true);
            litFlowerRenderer.enabled = true;
        }

        hasBeenLit = true;
        HideLightHint();
        HideSporeHint();

        if (ScoreManager.Instance != null && !hasBeenLitAlreadyCounted)
        {
            ScoreManager.Instance.AddPoint();
            hasBeenLitAlreadyCounted = true;
        }

        if (debugLogs)
            Debug.Log($"[SproutAndLightManager] {name} lit via {source}");
    }

    private void LateUpdate()
    {
        Vector3 anchorPos = hintAnchor != null ? hintAnchor.position : transform.position;
        Vector3 growthHintRise = new Vector3(0f, hintRisePerStage * _currentStage, 0f);

        if (_sporeHintGO != null && _sporeHintGO.activeSelf)
        {
            _sporeHintGO.transform.position = anchorPos + growthHintRise + sporeHintOffset;
            _sporeHintGO.transform.rotation = Quaternion.identity;
            _sporeHintGO.transform.localScale = GetSafeCounterScale(_sporeHintBaseLocalScale, sporeHintScale);
        }

        if (_lightHintGO != null && _lightHintGO.activeSelf)
        {
            _lightHintGO.transform.position = anchorPos + growthHintRise + lightHintOffset;
            _lightHintGO.transform.rotation = Quaternion.identity;
            _lightHintGO.transform.localScale = GetSafeCounterScale(_lightHintBaseLocalScale, lightHintScale);
        }
    }

    private Vector3 GetSafeCounterScale(Vector3 baseLocalScale, float multiplier)
    {
        Vector3 lossy = transform.lossyScale;

        float x = SafeScaledInverse(lossy.x, baseLocalScale.x * multiplier);
        float y = SafeScaledInverse(lossy.y, baseLocalScale.y * multiplier);

        float desiredZ = baseLocalScale.z;
        if (Mathf.Abs(desiredZ) < MIN_SCALE_EPSILON)
            desiredZ = 1f;

        float z;
        if (Mathf.Abs(lossy.z) < MIN_SCALE_EPSILON || float.IsNaN(lossy.z) || float.IsInfinity(lossy.z))
            z = desiredZ;
        else
            z = desiredZ / lossy.z;

        if (float.IsNaN(x) || float.IsInfinity(x))
            x = baseLocalScale.x * multiplier;

        if (float.IsNaN(y) || float.IsInfinity(y))
            y = baseLocalScale.y * multiplier;

        if (float.IsNaN(z) || float.IsInfinity(z))
            z = desiredZ;

        return new Vector3(x, y, z);
    }

    private float SafeScaledInverse(float value, float desiredScale)
    {
        if (Mathf.Abs(value) < MIN_SCALE_EPSILON || float.IsNaN(value) || float.IsInfinity(value))
            return desiredScale;

        return desiredScale / value;
    }

    public void ResetInitialPosition()
    {
        _initialWorldPos = transform.position;
    }

    public void ClearAllHints()
    {
        HideSporeHint();
        HideLightHint();
    }

    public void BrewFlower()
    {
        if (debugLogs)
            Debug.Log($"[BrewFlower] {name} — hasBeenLit={hasBeenLit}");

        if (hasBeenLit)
        {
            if (ScoreManager.Instance != null && hasBeenLitAlreadyCounted)
            {
                ScoreManager.Instance.points = Mathf.Max(0, ScoreManager.Instance.points - 1);
                ScoreManager.Instance.UpdatePointsText();
                hasBeenLitAlreadyCounted = false;
            }

            hasBeenLit = false;

            if (litFlowerRenderer != null)
                litFlowerRenderer.enabled = false;
        }
    }

    private void CacheSporeHintReference()
    {
        if (_sporeHintGO != null)
            return;

        Transform child = FindChildRecursive(transform, sporeChildName);
        if (child != null)
        {
            _sporeHintGO = child.gameObject;
            _sporeHintBaseLocalScale = child.localScale;
            _sporeHintGO.SetActive(false);

            if (debugLogs)
                Debug.Log($"[SproutAndLightManager] Found spore hint child '{sporeChildName}' on {name}");
        }
    }

    private void CacheLightHintReference()
    {
        if (_lightHintGO != null)
            return;

        Transform child = FindChildRecursive(transform, "LightPrompt");
        if (child != null)
        {
            _lightHintGO = child.gameObject;
            _lightHintBaseLocalScale = child.localScale;
            _lightHintGO.SetActive(false);

            if (debugLogs)
                Debug.Log($"[SproutAndLightManager] Found light hint child 'LightPrompt' on {name}");
        }
    }

    private void CachePlayerPromptReferences()
    {
        if (playerHeadOverride != null)
        {
            _cachedPlayerHead = playerHeadOverride;
            _cachedPlayerHeadCollider = _cachedPlayerHead.GetComponent<Collider2D>();
            _cachedPlayerRoot = _cachedPlayerHead.root;
            return;
        }

        if (_cachedPlayerRoot == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _cachedPlayerRoot = player.transform;
        }

        if (_cachedPlayerRoot != null && _cachedPlayerHead == null)
        {
            _cachedPlayerHead = FindChildRecursive(_cachedPlayerRoot, playerHeadName);
            if (_cachedPlayerHead != null)
                _cachedPlayerHeadCollider = _cachedPlayerHead.GetComponent<Collider2D>();
        }
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent == null || string.IsNullOrEmpty(childName))
            return null;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.name == childName)
                return child;

            Transform found = FindChildRecursive(child, childName);
            if (found != null)
                return found;
        }

        return null;
    }
}