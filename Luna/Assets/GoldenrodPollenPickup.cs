using UnityEngine;
using UnityEngine.UI;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.Collections;

public class GoldenrodPollenPickup : MonoBehaviour
{
    [Range(0f, 1f)]
    public float healPercentage = 0.25f;

    public float suppressionDuration = 3f;
    public AudioSource collectSFXSource;

    [Header("Health Bar Flash")]
    public float flashDuration = 0.4f;
    public float holdWhiteTime = 0.2f;

    private Image _healthBarFrontImage;
    private SpriteRenderer _healthBarFrontSpriteRenderer;

    private Color _cachedOriginalHealthBarColor = Color.white;
    private bool _hasCachedOriginalHealthBarColor = false;

    private Coroutine _healthBarFlashRoutine;

    void Start()
    {
        Debug.Log("GoldenrodPollenPickup Start running!");
        CacheHealthBarFront();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Transform playerRoot = other.transform.root;

        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = false;

        if (collectSFXSource != null)
            collectSFXSource.Play();

        Transform iconTransform = FindChildRecursive(playerRoot, "GoldenrodPollenLuna");
        if (iconTransform != null)
        {
            iconTransform.gameObject.SetActive(true);
            StartCoroutine(HideIconAfterDelay(iconTransform.gameObject, suppressionDuration));
        }

        Health health = playerRoot.GetComponent<Health>();
        if (health == null)
            health = playerRoot.GetComponentInChildren<Health>(true);

        if (health != null)
        {
            float healAmount = health.MaximumHealth * healPercentage;
            float newHealth = Mathf.Min(health.CurrentHealth + healAmount, health.MaximumHealth);
            health.SetHealth(newHealth, gameObject);

            MMProgressBar bar = FindObjectOfType<MMProgressBar>();
            if (bar != null)
                bar.UpdateBar(newHealth, 0f, health.MaximumHealth);
        }

        if (_healthBarFrontImage == null && _healthBarFrontSpriteRenderer == null)
            CacheHealthBarFront();

        if (_healthBarFrontImage != null || _healthBarFrontSpriteRenderer != null)
        {
            if (_healthBarFlashRoutine != null)
                StopCoroutine(_healthBarFlashRoutine);

            RestoreCachedHealthBarColor();
            _healthBarFlashRoutine = StartCoroutine(FlashHealthBar());
        }

        if (collectSFXSource != null && collectSFXSource.clip != null)
            Destroy(gameObject, collectSFXSource.clip.length);
        else
            Destroy(gameObject);
    }

    private void CacheHealthBarFront()
    {
        GameObject healthBarFront = GameObject.Find("HealthBarFront");
        if (healthBarFront == null)
        {
            Debug.LogWarning("HealthBarFront not found!");
            return;
        }

        _healthBarFrontImage = healthBarFront.GetComponent<Image>();
        _healthBarFrontSpriteRenderer = healthBarFront.GetComponent<SpriteRenderer>();

        if (_healthBarFrontImage == null && _healthBarFrontSpriteRenderer == null)
        {
            Debug.LogWarning("HealthBarFront has neither an Image nor a SpriteRenderer.");
            return;
        }

        CacheOriginalHealthBarColorIfNeeded();
    }

    private void CacheOriginalHealthBarColorIfNeeded()
    {
        if (_hasCachedOriginalHealthBarColor)
            return;

        if (_healthBarFrontImage != null)
        {
            _cachedOriginalHealthBarColor = _healthBarFrontImage.color;
            _hasCachedOriginalHealthBarColor = true;
            return;
        }

        if (_healthBarFrontSpriteRenderer != null)
        {
            _cachedOriginalHealthBarColor = _healthBarFrontSpriteRenderer.color;
            _hasCachedOriginalHealthBarColor = true;
        }
    }

    private IEnumerator FlashHealthBar()
{
    float totalTime = flashDuration + holdWhiteTime + flashDuration;
    float timer = 0f;

    while (timer < totalTime)
    {
        timer += Time.deltaTime;

        float t;

        if (timer < flashDuration)
        {
            // Fade to white
            t = timer / flashDuration;
            ApplyHealthBarColor(Color.Lerp(_cachedOriginalHealthBarColor, Color.white, t));
        }
        else if (timer < flashDuration + holdWhiteTime)
        {
            // Hold white
            ApplyHealthBarColor(Color.white);
        }
        else
        {
            // Fade back
            float backTime = timer - (flashDuration + holdWhiteTime);
            t = backTime / flashDuration;
            ApplyHealthBarColor(Color.Lerp(Color.white, _cachedOriginalHealthBarColor, t));
        }

        yield return null;
    }

    // Final restore (may get overwritten, but that's fine)
    ApplyHealthBarColor(_cachedOriginalHealthBarColor);
}

    private void ApplyHealthBarColor(Color color)
    {
        if (_healthBarFrontImage != null)
            _healthBarFrontImage.color = color;

        if (_healthBarFrontSpriteRenderer != null)
            _healthBarFrontSpriteRenderer.color = color;
    }

    private void RestoreCachedHealthBarColor()
    {
        if (_hasCachedOriginalHealthBarColor)
            ApplyHealthBarColor(_cachedOriginalHealthBarColor);
    }

    private IEnumerator HideIconAfterDelay(GameObject icon, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (icon != null)
            icon.SetActive(false);
    }

    public void PlayPickupSFX()
    {
        Debug.Log($"PlayPickupSFX called on {gameObject.name}");
        if (collectSFXSource != null)
            collectSFXSource.Play();
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