using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
public class TeacupToggle : MonoBehaviour
{
    [Header("Key Settings")]
    public KeyCode toggleKey = KeyCode.T;

    [Header("Fade Settings")]
    public float autoFadeDelay = 3f;
    public float fadeDuration = 1f;

    [Header("Audio Settings")]
    public AudioClip toggleSFX;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Sparkle Effect")]
    public GameObject sparkleObject;
    public float sparkleDuration = 2f;
    public float sparkleFadeTime = 0.5f;

    [Header("Game Feel")]
    public bool useLatency = false;
    public float latencyDuration = 0.1f;

    [Header("Hint Override")]
    public GameObject lilystoolHintIcon; // optional

    [Header("Wiggle Settings")]
    [Tooltip("Maximum wiggle angle in degrees (both directions)")]
    public float wiggleAngle = 25f; // default to 25° for visibility
    [Tooltip("How long each wiggle takes (back and forth)")]
    public float wiggleSpeed = 0.2f;
    [Tooltip("How many wiggles before pausing")]
    public int wiggleCount = 2;
    [Tooltip("How long to stay still after wiggles")]
    public float stillDuration = 1f;

    private SpriteRenderer sr;
    private Coroutine fadeRoutine;
    private Coroutine wiggleRoutine;
    private bool isVisible = true;
    private AudioSource audioSource;
    private bool playerNearby = false;
    private Quaternion initialRotation;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        initialRotation = transform.localRotation;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        if (sparkleObject != null)
            sparkleObject.SetActive(false);
    }

    void OnEnable()
    {
        if (isVisible) StartWiggle();
    }

    void OnDisable()
    {
        StopWiggle();
    }

    void Update()
    {
        // Suppress lilystool hint if near a teacup
        if (playerNearby && lilystoolHintIcon != null && lilystoolHintIcon.activeSelf)
        {
            lilystoolHintIcon.SetActive(false);
        }

        // ✅ Only allow toggle if Luna is nearby
        if (Input.GetKeyDown(toggleKey) && playerNearby)
        {
            PlaySFX();

            if (isVisible)
            {
                if (fadeRoutine != null) StopCoroutine(fadeRoutine);
                StartCoroutine(HideTeacupWithLatency());
            }
            else
            {
                if (fadeRoutine != null) StopCoroutine(fadeRoutine);
                SetAlpha(1f);
                isVisible = true;
                StartWiggle();
            }
        }
    }

    // 🔧 Extra suppression layer (runs after all Update calls)
    void LateUpdate()
    {
        if (playerNearby && lilystoolHintIcon != null && lilystoolHintIcon.activeSelf)
        {
            lilystoolHintIcon.SetActive(false);
        }
    }

    private IEnumerator HideTeacupWithLatency()
    {
        if (useLatency && latencyDuration > 0f)
            yield return new WaitForSeconds(latencyDuration);

        SetAlpha(0f);
        isVisible = false;
        StopWiggle();

        if (sparkleObject != null)
            StartCoroutine(SparkleRoutine());

        fadeRoutine = StartCoroutine(FadeBackInAfterDelay());
    }

    private IEnumerator FadeBackInAfterDelay()
    {
        yield return new WaitForSeconds(autoFadeDelay);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            SetAlpha(t);
            yield return null;
        }

        isVisible = true;
        StartWiggle();
        fadeRoutine = null;
    }

    private IEnumerator SparkleRoutine()
    {
        sparkleObject.SetActive(true);
        SetObjectAlpha(sparkleObject, 1f);

        yield return new WaitForSeconds(sparkleDuration);

        float elapsed = 0f;
        while (elapsed < sparkleFadeTime)
        {
            elapsed += Time.deltaTime;
            float t = 1f - Mathf.Clamp01(elapsed / sparkleFadeTime);
            SetObjectAlpha(sparkleObject, t);
            yield return null;
        }

        sparkleObject.SetActive(false);
    }

    private void SetAlpha(float value)
    {
        Color c = sr.color;
        c.a = value;
        sr.color = c;
    }

    private void SetObjectAlpha(GameObject obj, float alpha)
    {
        if (obj == null) return;

        var renderers = obj.GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in renderers)
        {
            Color c = r.color;
            c.a = alpha;
            r.color = c;
        }
    }

    private void PlaySFX()
    {
        if (toggleSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(toggleSFX, sfxVolume);
        }
    }

    // 🔑 Player proximity detection
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerNearby = false;
    }

    // 🔑 Wiggle loop
    private void StartWiggle()
    {
        if (wiggleRoutine != null) StopCoroutine(wiggleRoutine);
        wiggleRoutine = StartCoroutine(WiggleLoop());
    }

    private void StopWiggle()
    {
        if (wiggleRoutine != null) StopCoroutine(wiggleRoutine);
        wiggleRoutine = null;
        transform.localRotation = initialRotation; // Reset rotation
    }

    private IEnumerator WiggleLoop()
    {
        while (isVisible)
        {
            for (int i = 0; i < wiggleCount; i++)
            {
                yield return RotateToAngle(wiggleAngle);
                yield return RotateToAngle(-wiggleAngle);
                yield return RotateToAngle(0f);
            }
            yield return new WaitForSeconds(stillDuration);
        }
    }

    private IEnumerator RotateToAngle(float targetAngle)
    {
        float elapsed = 0f;
        Quaternion startRot = transform.localRotation;
        Quaternion endRot = Quaternion.Euler(0, 0, targetAngle);

        while (elapsed < wiggleSpeed)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / wiggleSpeed);
            transform.localRotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }
    }
}
