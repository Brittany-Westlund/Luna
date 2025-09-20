using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;

public class LunaHealthWarning : MonoBehaviour
{
    [Header("Scene Gating")]
    [Tooltip("If true, this script only runs in scenes whose name starts with Gameplay Scene Prefix.")]
    public bool onlyInGameplayScenes = true;
    [Tooltip("Gameplay scene name prefix (e.g., 'Level', 'World').")]
    public string gameplayScenePrefix = "Level";

    [Header("Lookup (optional)")]
    [Tooltip("Tag on the MMProgressBar GameObject for health. Leave empty to auto-detect.")]
    public string healthBarTag = "";
    [Tooltip("Tag on the avatar head Image/Sprite. Leave empty to auto-detect by name contains 'avatar'.")]
    public string avatarHeadTag = "";

    [Header("Trigger")]
    [Range(0f,1f)] public float lowHealthThreshold = 0.2f;  // 20%
    [Tooltip("Delay before this system can activate (lets menus/UI load).")]
    public float activationDelay = 5f;
    [Tooltip("Delay between pulses while health stays low.")]
    public float warningRepeatInterval = 2.5f;

    [Header("Scale Pulse")]
    public bool useScale = true;
    public float scaleAmount = 1.2f;
    public float scaleDuration = 0.2f;
    public int scaleRepeats = 2;

    [Header("Color Blink")]
    public bool useBlink = true;
    public Color warningColor = Color.red;
    public float blinkOnTime = 0.12f;
    public float blinkOffTime = 0.12f;
    public int blinkRepeats = 4;

    [Header("Audio")]
    public AudioSource warningAudio; // optional

    // internals
    private bool _activated;
    private bool _warningActive;
    private float _prevNorm = 1f;

    private Health _health;
    private MMProgressBar _healthBar;

    private Transform _avatarHead;
    private Image _avatarImage;            // UI avatar
    private SpriteRenderer _avatarSprite;  // world-space fallback
    private Color _originalColor;
    private Vector3 _originalScale;

    private Coroutine _activateCo;
    private Coroutine _warningCo;

    void Awake()
    {
        _health = GetComponent<Health>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (_activateCo != null) StopCoroutine(_activateCo);
        if (_warningCo != null) StopCoroutine(_warningCo);
    }

    void Start()
    {
        if (warningAudio != null)
        {
            warningAudio.playOnAwake = false;
            warningAudio.loop = false;
            warningAudio.Stop();
        }

        _activateCo = StartCoroutine(ActivateAfterDelay());
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // Always try to relink after load
        RelinkSceneObjects();
        CacheAvatarVisuals();

        // Gate by scene name if requested
        if (onlyInGameplayScenes && !s.name.StartsWith(gameplayScenePrefix))
        {
            _activated = false;
            StopWarningIfRunning();
            return;
        }

        // Arm only if we have a source of health AND some UI target
        bool uiPresent = (_healthBar != null || _avatarImage != null || _avatarSprite != null);
        _activated = uiPresent && _health != null && _health.MaximumHealth > 0f;
        _prevNorm = GetNormalizedHealth();
    }

    private IEnumerator ActivateAfterDelay()
    {
        // Early relink (e.g., if starting directly in a gameplay scene)
        RelinkSceneObjects();

        yield return new WaitForSeconds(activationDelay);

        RelinkSceneObjects();  // try again after delay (UI may have been instantiated)
        CacheAvatarVisuals();

        // Scene gating check on current scene
        var current = SceneManager.GetActiveScene().name;
        if (onlyInGameplayScenes && !current.StartsWith(gameplayScenePrefix))
        {
            _activated = false;
            yield break;
        }

        bool uiPresent = (_healthBar != null || _avatarImage != null || _avatarSprite != null);
        _activated = uiPresent && _health != null && _health.MaximumHealth > 0f;
        _prevNorm = GetNormalizedHealth();
    }

    void Update()
    {
        if (!_activated) return;

        float norm = GetNormalizedHealth();

        if (norm <= lowHealthThreshold && _prevNorm > lowHealthThreshold && !_warningActive)
        {
            _warningActive = true;
            if (_warningCo != null) StopCoroutine(_warningCo);
            _warningCo = StartCoroutine(WarningLoop());
        }
        else if (norm > lowHealthThreshold && _warningActive)
        {
            StopWarningIfRunning();
        }

        _prevNorm = norm;
    }

    // ===== Linking / Setup =====

    private GameObject SafeFindWithTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return null;
        try { return GameObject.FindWithTag(tag); }
        catch (UnityException) { return null; } // tag not defined → don't crash
    }

    private void RelinkSceneObjects()
    {
        // Health bar (tagged → fallback auto-detect)
        _healthBar = null;
        var taggedHB = SafeFindWithTag(healthBarTag);
        if (taggedHB != null) _healthBar = taggedHB.GetComponent<MMProgressBar>();
        if (_healthBar == null) _healthBar = FindObjectOfType<MMProgressBar>();

        // Avatar (Image preferred; fallback to SpriteRenderer)
        _avatarHead = null; _avatarImage = null; _avatarSprite = null;

        var taggedHead = SafeFindWithTag(avatarHeadTag);
        if (taggedHead != null)
        {
            _avatarHead = taggedHead.transform;
            _avatarImage = taggedHead.GetComponent<Image>();
            if (_avatarImage == null) _avatarSprite = taggedHead.GetComponent<SpriteRenderer>();
        }

        if (_avatarHead == null)
        {
            // Find any Image named/containing "avatar"
            foreach (var img in FindObjectsOfType<Image>(true))
            {
                if (img.gameObject.name.ToLower().Contains("avatar"))
                {
                    _avatarHead = img.transform;
                    _avatarImage = img;
                    break;
                }
            }
        }

        if (_avatarHead == null)
        {
            // Fallback to SpriteRenderer containing "avatar"
            foreach (var sr in FindObjectsOfType<SpriteRenderer>(true))
            {
                if (sr.gameObject.name.ToLower().Contains("avatar"))
                {
                    _avatarHead = sr.transform;
                    _avatarSprite = sr;
                    break;
                }
            }
        }
    }

    private void CacheAvatarVisuals()
    {
        if (_avatarHead != null) _originalScale = _avatarHead.localScale;
        if (_avatarImage != null) _originalColor = _avatarImage.color;
        else if (_avatarSprite != null) _originalColor = _avatarSprite.color;
    }

    private float GetNormalizedHealth()
    {
        // Prefer the actual Health component as source of truth
        if (_health != null && _health.MaximumHealth > 0f)
            return Mathf.Clamp01(_health.CurrentHealth / _health.MaximumHealth);
        // Fallback to bar if needed
        if (_healthBar != null)
            return Mathf.Clamp01(_healthBar.BarProgress);
        // No data? Act healthy to silence warnings outside gameplay
        return 1f;
    }

    // ===== Warning Logic =====

    private IEnumerator WarningLoop()
    {
        while (GetNormalizedHealth() <= lowHealthThreshold)
        {
            if (warningAudio) warningAudio.Play();

            // Run scale+blink in parallel
            Coroutine scaleCo = null, blinkCo = null;

            if (useScale && _avatarHead != null)
                scaleCo = StartCoroutine(ScalePulse());

            if (useBlink && (_avatarImage != null || _avatarSprite != null))
                blinkCo = StartCoroutine(ColorBlinkPulse());

            if (scaleCo != null) yield return scaleCo;
            if (blinkCo != null) yield return blinkCo;

            if (GetNormalizedHealth() <= lowHealthThreshold)
                yield return new WaitForSeconds(warningRepeatInterval);
        }

        ResetAvatar();
        _warningActive = false;
        _warningCo = null;
    }

    private void StopWarningIfRunning()
    {
        _warningActive = false;
        if (_warningCo != null) { StopCoroutine(_warningCo); _warningCo = null; }
        if (warningAudio && warningAudio.isPlaying) warningAudio.Stop();
        ResetAvatar();
    }

    private IEnumerator ScalePulse()
    {
        _avatarHead.localScale = _originalScale;

        for (int i = 0; i < scaleRepeats; i++)
        {
            yield return ScaleTo(_originalScale * scaleAmount, scaleDuration);
            yield return ScaleTo(_originalScale, scaleDuration);
        }
    }

    private IEnumerator ScaleTo(Vector3 target, float dur)
    {
        if (_avatarHead == null) yield break;
        dur = Mathf.Max(0.0001f, dur);
        Vector3 start = _avatarHead.localScale;
        float t = 0f;
        while (t < dur)
        {
            _avatarHead.localScale = Vector3.Lerp(start, target, t / dur);
            t += Time.deltaTime;
            yield return null;
        }
        _avatarHead.localScale = target;
    }

    private IEnumerator ColorBlinkPulse()
    {
        for (int i = 0; i < blinkRepeats; i++)
        {
            SetAvatarColor(warningColor);
            yield return new WaitForSeconds(blinkOnTime);
            SetAvatarColor(_originalColor);
            yield return new WaitForSeconds(blinkOffTime);
        }
    }

    private void SetAvatarColor(Color c)
    {
        if (_avatarImage) _avatarImage.color = c;
        else if (_avatarSprite) _avatarSprite.color = c;
    }

    private void ResetAvatar()
    {
        if (_avatarHead != null) _avatarHead.localScale = _originalScale;
        SetAvatarColor(_originalColor);
    }
}


/* using UnityEngine;
using UnityEngine.UI;  // For Image component
using System.Collections;
using MoreMountains.Tools;

public class LunaHealthWarning : MonoBehaviour
{
    public MMProgressBar healthBar;               // Reference to Luna's health bar
    public Transform avatarHead;                  // Transform for the AvatarHead GameObject
    public AudioSource warningAudio;              // AudioSource for the warning sound effect
    public float scaleAmount = 1.2f;              // Amount to scale the AvatarHead by (e.g., 1.2 for 120% size)
    public float scaleDuration = 0.2f;            // Duration of each scale up/down (seconds)
    public int scaleRepeats = 2;                  // Number of times to scale up/down each trigger
    public float warningRepeatInterval = 2.5f;    // Interval in seconds to repeat the warning
    public Color warningColor = Color.red;        // Color to turn the AvatarHead when scaling
    public float activationDelay = 5f;            // Delay in seconds before activating the script

    private bool warningActive = false;           // Track if the warning coroutine is active
    private bool isActivated = false;             // Tracks if script has fully activated
    private Image avatarImage;                    // Reference to the AvatarHead's Image component
    private Color originalColor;                  // Store the original color of AvatarHead
    private Vector3 originalScale;                // Store the original scale of AvatarHead
    private float previousHealth = 1.0f;          // Track previous health to detect changes

    private void Start()
    {
        StartCoroutine(ActivateAfterDelay()); // Start coroutine to enable script after delay

        // Ensure warning audio doesn’t play on awake and doesn't loop
        if (warningAudio != null)
        {
            warningAudio.playOnAwake = false;
            warningAudio.loop = false;  // Disable looping so we can control playback
            warningAudio.Stop();
        }
    }

    private IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);

        // Initialize avatar visuals
        avatarImage = avatarHead.GetComponent<Image>();
        if (avatarImage != null)
        {
            originalColor = avatarImage.color;  // Save the original color
        }
        originalScale = avatarHead.localScale; // Store original scale

        // Set script as fully activated
        isActivated = true;
        previousHealth = healthBar.BarProgress; // Set previousHealth after delay
    }

    private void Update()
    {
        // Only proceed if the script has fully activated after delay
        if (!isActivated) return;

        float currentHealth = healthBar.BarProgress;

        // Only start the warning if health has just dropped below 20%
        if (currentHealth <= 0.2f && previousHealth > 0.2f && !warningActive)
        {
            StartCoroutine(WarningRoutine());
            warningActive = true;
        }
        // Stop the warning if health goes back above 20%
        else if (currentHealth > 0.2f && warningActive)
        {
            warningActive = false;
            StopAllCoroutines();
            if (warningAudio != null && warningAudio.isPlaying) warningAudio.Stop();
            ResetAvatarVisuals();  // Ensure visuals reset when warning stops
        }

        // Update previous health to the current value for the next frame
        previousHealth = currentHealth;
    }

    private IEnumerator WarningRoutine()
    {
        while (healthBar.BarProgress <= 0.2f)
        {
            // Play the scaling animation and sound effect
            StartCoroutine(ScaleAvatarHead());

            if (warningAudio != null)
            {
                warningAudio.Play();
            }

            // Wait for the specified repeat interval before triggering again
            yield return new WaitForSeconds(warningRepeatInterval);
        }
    }

    private IEnumerator ScaleAvatarHead()
    {
        // Ensure the avatar starts at its original scale
        avatarHead.localScale = originalScale;

        // Set the avatar color to the warning color
        if (avatarImage != null)
        {
            avatarImage.color = warningColor;
        }

        for (int i = 0; i < scaleRepeats; i++)
        {
            // Scale up
            yield return ScaleTo(originalScale * scaleAmount);

            // Scale back to original size
            yield return ScaleTo(originalScale);
        }

        // Reset the avatar color back to its original color
        ResetAvatarVisuals();
    }

    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        Vector3 startScale = avatarHead.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < scaleDuration)
        {
            // Interpolate between the start and target scale over scaleDuration
            avatarHead.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / scaleDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        avatarHead.localScale = targetScale;
    }

    private void ResetAvatarVisuals()
    {
        // Reset the avatar color and scale
        if (avatarImage != null)
        {
            avatarImage.color = originalColor;
        }
        avatarHead.localScale = originalScale; // Ensure it resets to original scale
    }
}
*/