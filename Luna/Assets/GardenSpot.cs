using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GardenSpot : MonoBehaviour
{
    [Tooltip("Child Transform where the flower will snap to.")]
    public Transform plantingPoint;

    [Tooltip("SpriteRenderer object to tint for highlight.")]
    public GameObject highlightObject;

    [Header("Reveal Behavior")]
    [Tooltip("If true, this garden starts hidden and must be revealed by the butterfly first.")]
    public bool requiresButterflyReveal = true;

    [Header("Flower Interaction Prompt")]
    [Tooltip("If true, this garden can show the F prompt when the player is holding a flower.")]
    public bool allowFlowerPrompt = true;

    [Tooltip("Optional grass GameObject.")]
    public GameObject grownGrassObject;

    [Tooltip("Optional grass SpriteRenderer. Preferred if the grass object stays active and fades in via alpha.")]
    public SpriteRenderer grownGrassRenderer;

    [Range(0f, 1f)]
    [Tooltip("Grass alpha must be at or above this value for the prompt to show.")]
    public float promptGrassAlphaThreshold = 0.95f;

    [Header("Highlight Fade")]
    public float fadeDuration = 0.3f;

    [Header("Sparkles")]
    [Tooltip("Assign the parent Sparkles object here.")]
    public GameObject sparkleGroup;

    [Header("Sparkle Scale")]
    public float sparkleIdleScale = 1f;
    public float sparkleActiveScale = 1.5f;
    public float sparkleScaleLerpSpeed = 6f;

    [Header("Sparkle Brightness")]
    public Color sparkleIdleColor = new Color(0.8f, 0.8f, 0.8f, 1f);
    public Color sparkleActiveColor = Color.white;
    public float sparkleColorLerpSpeed = 6f;

    [Header("Player Detection")]
    public string playerTag = "Player";

    [Header("Debug")]
    public bool debugLogs = false;

    private SpriteRenderer _highlightRenderer;
    private Color _originalColor;
    private Color _currentTarget;
    private float _fadeTimer;
    private bool _isFading;
    private bool _isHighlighted;

    private GameObject plantedFlower;

    private Transform[] _sparkleTransforms;
    private SpriteRenderer[] _sparkleRenderers;
    private Pulsate[] _sparklePulsates;
    private Vector3[] _sparkleBaseScales;

    private float _currentSparkleScaleMultiplier = 1f;
    private float _targetSparkleScaleMultiplier = 1f;
    private Color _currentSparkleColor;
    private Color _targetSparkleColor;
    private bool _sparkleIsActive;

    private Collider2D _gardenTrigger;
    private GameObject _player;
    private Collider2D[] _playerColliders = new Collider2D[0];

    private CustomInteractionFeedback _fPrompt;
    private FlowerHolder _playerFlowerHolder;

    private void Awake()
    {
        _gardenTrigger = GetComponent<Collider2D>();
        if (_gardenTrigger != null && !_gardenTrigger.isTrigger)
            _gardenTrigger.isTrigger = true;

        if (highlightObject != null)
        {
            _highlightRenderer = highlightObject.GetComponent<SpriteRenderer>();
            if (_highlightRenderer != null)
                _originalColor = _highlightRenderer.color;
        }

        _currentTarget = _originalColor;

        if (sparkleGroup == null)
        {
            Transform found = transform.Find("Sparkles");
            if (found != null)
                sparkleGroup = found.gameObject;
        }

        if (grownGrassRenderer == null && grownGrassObject != null)
            grownGrassRenderer = grownGrassObject.GetComponent<SpriteRenderer>();

        CacheSparkles();

        _currentSparkleScaleMultiplier = sparkleIdleScale;
        _targetSparkleScaleMultiplier = sparkleIdleScale;
        _currentSparkleColor = sparkleIdleColor;
        _targetSparkleColor = sparkleIdleColor;

        ApplySparkleImmediate();

        if (sparkleGroup != null)
            sparkleGroup.SetActive(!requiresButterflyReveal);

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject c = transform.GetChild(i).gameObject;
            SproutAndLightManager mgr = c.GetComponent<SproutAndLightManager>();
            if (mgr != null)
            {
                plantedFlower = c;
                mgr.isPlanted = true;
                break;
            }
        }

        _fPrompt = GetComponentInChildren<CustomInteractionFeedback>(true);

        RebindPlayer();
    }

    private void OnEnable()
    {
        RebindPlayer();
        RefreshSparkleProximityState();
        UpdatePrompt();
    }

    private void Update()
    {
        if (_player == null || !_player.activeInHierarchy || _playerFlowerHolder == null)
            RebindPlayer();

        RefreshSparkleProximityState();
        UpdateHighlight();
        UpdateSparkles();
        UpdatePrompt();
    }

    private void RebindPlayer()
    {
        _player = GameObject.FindGameObjectWithTag(playerTag);

        _playerColliders = _player != null
            ? _player.GetComponentsInChildren<Collider2D>(true)
            : new Collider2D[0];

        _playerFlowerHolder = _player != null
            ? _player.GetComponent<FlowerHolder>()
            : null;
    }

    private bool IsPlayerTouching()
    {
        if (_gardenTrigger == null)
            return false;

        for (int i = 0; i < _playerColliders.Length; i++)
        {
            Collider2D pc = _playerColliders[i];
            if (pc != null && _gardenTrigger.IsTouching(pc))
                return true;
        }

        return false;
    }

    private void RefreshSparkleProximityState()
    {
        if (sparkleGroup == null || !sparkleGroup.activeInHierarchy)
        {
            SetSparkleActive(false);
            return;
        }

        SetSparkleActive(IsPlayerTouching());
    }

    private bool IsGardenGrown()
    {
        if (grownGrassRenderer != null)
            return grownGrassRenderer.color.a >= promptGrassAlphaThreshold;

        if (grownGrassObject != null)
        {
            SpriteRenderer sr = grownGrassObject.GetComponent<SpriteRenderer>();
            if (sr != null)
                return sr.color.a >= promptGrassAlphaThreshold;

            return grownGrassObject.activeInHierarchy;
        }

        if (plantedFlower != null)
        {
            SproutAndLightManager sprout = plantedFlower.GetComponent<SproutAndLightManager>();
            if (sprout != null)
                return sprout.IsFullyGrown;
        }

        return false;
    }

    private void UpdatePrompt()
    {
        if (_fPrompt == null)
            return;

        bool show =
            allowFlowerPrompt &&
            _playerFlowerHolder != null &&
            _playerFlowerHolder.HasFlower &&
            IsPlayerTouching() &&
            IsGardenGrown();

        if (show)
        {
            if (!_fPrompt.gameObject.activeSelf)
                _fPrompt.TurnSelfOn();
        }
        else
        {
            if (_fPrompt.gameObject.activeSelf)
                _fPrompt.TurnSelfOff();
        }
    }

    private void CacheSparkles()
    {
        if (sparkleGroup == null)
        {
            _sparkleTransforms = new Transform[0];
            _sparkleRenderers = new SpriteRenderer[0];
            _sparklePulsates = new Pulsate[0];
            _sparkleBaseScales = new Vector3[0];
            return;
        }

        int count = sparkleGroup.transform.childCount;

        _sparkleTransforms = new Transform[count];
        _sparkleRenderers = new SpriteRenderer[count];
        _sparklePulsates = new Pulsate[count];
        _sparkleBaseScales = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            Transform child = sparkleGroup.transform.GetChild(i);
            _sparkleTransforms[i] = child;
            _sparkleRenderers[i] = child.GetComponent<SpriteRenderer>();
            _sparklePulsates[i] = child.GetComponent<Pulsate>();
            _sparkleBaseScales[i] = child.localScale;
        }
    }

    private void UpdateSparkles()
    {
        if (sparkleGroup == null || _sparkleTransforms == null)
            return;

        _currentSparkleScaleMultiplier = Mathf.Lerp(
            _currentSparkleScaleMultiplier,
            _targetSparkleScaleMultiplier,
            Time.deltaTime * sparkleScaleLerpSpeed
        );

        _currentSparkleColor = Color.Lerp(
            _currentSparkleColor,
            _targetSparkleColor,
            Time.deltaTime * sparkleColorLerpSpeed
        );

        for (int i = 0; i < _sparkleTransforms.Length; i++)
        {
            Transform t = _sparkleTransforms[i];
            if (t == null)
                continue;

            Vector3 baseScale = _sparkleBaseScales[i] * _currentSparkleScaleMultiplier;

            if (_sparklePulsates[i] != null)
            {
                if (_sparklePulsates[i].useExternalBaseScale)
                    _sparklePulsates[i].SetBaseScale(baseScale);
                else
                    t.localScale = baseScale;

                if (_sparklePulsates[i].useExternalBaseColor)
                    _sparklePulsates[i].SetBaseColor(_currentSparkleColor);
                else if (_sparkleRenderers[i] != null)
                    _sparkleRenderers[i].color = _currentSparkleColor;
            }
            else
            {
                t.localScale = baseScale;

                if (_sparkleRenderers[i] != null)
                    _sparkleRenderers[i].color = _currentSparkleColor;
            }
        }
    }

    private void ApplySparkleImmediate()
    {
        if (_sparkleTransforms == null)
            return;

        for (int i = 0; i < _sparkleTransforms.Length; i++)
        {
            if (_sparkleTransforms[i] == null)
                continue;

            Vector3 baseScale = _sparkleBaseScales[i];

            if (_sparklePulsates[i] != null)
            {
                if (_sparklePulsates[i].useExternalBaseScale)
                    _sparklePulsates[i].SetBaseScale(baseScale);
                else
                    _sparkleTransforms[i].localScale = baseScale;

                if (_sparklePulsates[i].useExternalBaseColor)
                    _sparklePulsates[i].SetBaseColor(_currentSparkleColor);
                else if (_sparkleRenderers[i] != null)
                    _sparkleRenderers[i].color = _currentSparkleColor;
            }
            else
            {
                _sparkleTransforms[i].localScale = baseScale;

                if (_sparkleRenderers[i] != null)
                    _sparkleRenderers[i].color = _currentSparkleColor;
            }
        }
    }

    private void UpdateHighlight()
    {
        if (!_isFading || _highlightRenderer == null)
            return;

        _fadeTimer += Time.deltaTime / fadeDuration;
        _highlightRenderer.color = Color.Lerp(_highlightRenderer.color, _currentTarget, _fadeTimer);

        if (_fadeTimer >= 1f)
        {
            _highlightRenderer.color = _currentTarget;
            _isFading = false;
        }
    }

    public void SetHighlight(bool on)
    {
        if (_highlightRenderer == null)
            return;

        if (_isHighlighted == on)
            return;

        _isHighlighted = on;
        _currentTarget = on ? Color.white : _originalColor;
        _fadeTimer = 0f;
        _isFading = true;
    }

    public void SetSparkleActive(bool active)
    {
        _sparkleIsActive = active;
        _targetSparkleScaleMultiplier = active ? sparkleActiveScale : sparkleIdleScale;
        _targetSparkleColor = active ? sparkleActiveColor : sparkleIdleColor;

        if (debugLogs)
            Debug.Log($"[GardenSpot] {name} SetSparkleActive({active})");
    }

    public void Reveal()
    {
        if (sparkleGroup != null)
        {
            sparkleGroup.SetActive(true);
            ApplySparkleImmediate();
            RefreshSparkleProximityState();
        }
    }

    public void Hide()
    {
        if (sparkleGroup != null)
            sparkleGroup.SetActive(false);
    }

    public void SetPlantedFlower(GameObject flower)
    {
        plantedFlower = flower;
    }

    public GameObject GetPlantedFlower()
    {
        return plantedFlower;
    }

    public void ClearPlantedFlower()
    {
        plantedFlower = null;
    }

    public Transform GetPlantingPoint()
    {
        return plantingPoint;
    }

    public static void NormalizeTransform(Transform t)
    {
        t.localScale = Vector3.one;
        t.localRotation = Quaternion.identity;
    }
}