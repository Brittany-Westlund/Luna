using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    [Tooltip("Child Transform where the flower will snap to.")]
    public Transform plantingPoint;

    [Tooltip("SpriteRenderer object to tint for highlight.")]
    public GameObject highlightObject;

    [Header("Highlight Fade")]
    public float fadeDuration = 0.3f;
    public ParticleSystem sparkleFX;
    private bool revealed;

    private SpriteRenderer _highlightRenderer;
    private Color _originalColor;
    private Color _currentTarget;
    private float _fadeTimer;
    private bool _isFading;

    // Which flower is planted here
    private GameObject plantedFlower;

    void Awake()
    {
        if (highlightObject != null)
        {
            _highlightRenderer = highlightObject.GetComponent<SpriteRenderer>();
            if (_highlightRenderer != null)
                _originalColor = _highlightRenderer.color;
        }

        _currentTarget = _originalColor;

        // Auto-register any pre-placed child flower
        for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i).gameObject;
            if (c.GetComponent<SproutAndLightManager>() != null)
            {
                plantedFlower = c;
                var mgr = c.GetComponent<SproutAndLightManager>();
                mgr.isPlanted = true;
                break;
            }
        }
    }

    void Update()
    {
        if (_isFading && _highlightRenderer != null)
        {
            _fadeTimer += Time.deltaTime / fadeDuration;
            _highlightRenderer.color = Color.Lerp(_highlightRenderer.color, _currentTarget, _fadeTimer);

            if (_fadeTimer >= 1f)
            {
                _highlightRenderer.color = _currentTarget;
                _isFading = false;
            }
        }
    }

    public void SetHighlight(bool on)
    {
        if (_highlightRenderer == null) return;

        _currentTarget = on ? Color.white : _originalColor;
        _fadeTimer = 0f;
        _isFading = true;

        // If disabled, just snap
        if (!gameObject.activeInHierarchy)
        {
            _highlightRenderer.color = _currentTarget;
            _isFading = false;
        }
    }

    public void SetPlantedFlower(GameObject flower) => plantedFlower = flower;
    public GameObject GetPlantedFlower() => plantedFlower;
    public void ClearPlantedFlower() => plantedFlower = null;
    public Transform GetPlantingPoint() => plantingPoint;

    public static void NormalizeTransform(Transform t)
    {
        t.localScale = Vector3.one;
        t.localRotation = Quaternion.identity;
    }
    public void Reveal()
    {
        if (revealed) return;
        sparkleFX.Play();
        revealed = true;
    }

    public void Hide()
    {
        if (!revealed) return;
        sparkleFX.Stop();
        revealed = false;
    }
}



/* using UnityEngine;
using System.Collections;

public class GardenSpot : MonoBehaviour
{
    [Tooltip("Child Transform where the flower will snap to.")]
    public Transform plantingPoint;

    [Tooltip("SpriteRenderer object to tint for highlight.")]
    public GameObject highlightObject;

    [Tooltip("Seconds for highlight fade in/out")]
    public float fadeDuration = 0.3f;

    private SpriteRenderer _highlightRenderer;
    private Color          _originalColor;
    private Coroutine      _fadeRoutine;

    // Which flower is planted here
    private GameObject plantedFlower;

    void Awake()
    {
        if (highlightObject != null)
        {
            _highlightRenderer = highlightObject.GetComponent<SpriteRenderer>();
            if (_highlightRenderer != null)
                _originalColor = _highlightRenderer.color;
        }

        // Auto-register any pre-placed child flower
        for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i).gameObject;
            if (c.GetComponent<SproutAndLightManager>() != null)
            {
                plantedFlower = c;
                var mgr = c.GetComponent<SproutAndLightManager>();
                mgr.isPlanted = true;
                break;
            }
        }
    }

    /// <summary>Fade the highlight in or out.</summary>
    public void SetHighlight(bool on)
{
    if (_highlightRenderer == null) return;

    Color targetColor = on ? Color.white : _originalColor;

    if (!isActiveAndEnabled)
    {
        // GameObject is inactive, just snap color
        _highlightRenderer.color = targetColor;
        return;
    }

    if (_fadeRoutine != null)
        StopCoroutine(_fadeRoutine);
    _fadeRoutine = StartCoroutine(FadeToColor(targetColor, fadeDuration));
}


    private IEnumerator FadeToColor(Color target, float duration)
    {
        Color start = _highlightRenderer.color;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            _highlightRenderer.color = Color.Lerp(start, target, t);
            yield return null;
        }

        _highlightRenderer.color = target;
        _fadeRoutine = null;
    }

    // Flower helpers (unchanged)
    public void SetPlantedFlower(GameObject flower) => plantedFlower = flower;
    public GameObject GetPlantedFlower() => plantedFlower;
    public void ClearPlantedFlower() => plantedFlower = null;
    public Transform GetPlantingPoint() => plantingPoint;

    public static void NormalizeTransform(Transform t)
    {
        t.localScale = Vector3.one;
        t.localRotation = Quaternion.identity;
    }
}


/*
using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    [Tooltip("Child Transform where the flower will snap to.")]
    public Transform plantingPoint;

    [Tooltip("SpriteRenderer object to tint for highlight.")]
    public GameObject highlightObject;

    private SpriteRenderer _highlightRenderer;
    private Color          _originalColor;

    // Which flower is planted here
    private GameObject plantedFlower;

    void Awake()
    {
        // Cache the SpriteRenderer & its original color
        if (highlightObject != null)
        {
            _highlightRenderer = highlightObject.GetComponent<SpriteRenderer>();
            if (_highlightRenderer != null)
                _originalColor = _highlightRenderer.color;
        }

        // Auto‑register any pre‑placed child flower
        for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i).gameObject;
            if (c.GetComponent<SproutAndLightManager>() != null)
            {
                plantedFlower = c;
                var mgr = c.GetComponent<SproutAndLightManager>();
                mgr.isPlanted = true;
                break;
            }
        }
    }

    /// <summary>Called every frame by the manager.</summary>
    public void SetHighlight(bool on)
    {
        if (_highlightRenderer == null) return;
        _highlightRenderer.color = on ? Color.white : _originalColor;
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
*/
