using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[DisallowMultipleComponent]
public class GardenGrowth : MonoBehaviour
{
    [Header("References")]
    public GameObject grassObject;
    public SpriteRenderer grassRenderer;
    public GardenSpot gardenSpot;

    [Header("Initial State")]
    public bool startGrown = false;

    [Header("Save Settings")]
    public string saveKey = "";
    public bool includeSceneNameInKey = true;

    [Header("Growth Settings")]
    public float restDuration = 3f;
    public float fadeDuration = 1f;
    public bool onlyGrowOnce = true;

    [Header("Colors")]
    public Color grownIdleColor = new Color(0.7f, 0.7f, 0.7f, 1f);
    public Color grownCollideColor = Color.white;
    public float colorShiftDuration = 0.25f;

    [Header("Audio")]
    public AudioClip growthSFX;
    [Range(0f, 1f)] public float growthVolume = 1f;
    public float soundDelay = 0f;

    private Coroutine restCoroutine;
    private Coroutine fadeCoroutine;
    private Coroutine colorShiftCoroutine;
    private AudioSource audioSource;

    private bool hasGrown = false;
    private bool isGrowing = false;
    private bool feetInside = false;

    private string resolvedSaveKey;

    private void Awake()
    {
        if (grassRenderer == null && grassObject != null)
            grassRenderer = grassObject.GetComponent<SpriteRenderer>();

        if (gardenSpot == null)
            gardenSpot = GetComponent<GardenSpot>();

        resolvedSaveKey = BuildResolvedSaveKey();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;

        hasGrown = LoadGrowthState();
        isGrowing = false;
        feetInside = false;

        if (hasGrown)
        {
            ApplyGrownVisualImmediate();

            if (gardenSpot != null)
            {
                gardenSpot.Reveal();
                gardenSpot.SetSparkleActive(false);
            }
        }
        else
        {
            ApplyHiddenVisual();

            if (gardenSpot != null)
                gardenSpot.Hide();
        }
    }

    private void LateUpdate()
    {
        if (!hasGrown || isGrowing)
            return;

        if (grassObject != null && !grassObject.activeSelf)
            grassObject.SetActive(true);

        if (grassRenderer != null)
        {
            Color c = grassRenderer.color;
            c.a = 1f;
            grassRenderer.color = c;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerFeet"))
            return;

        if (other.GetComponentInParent<LunaRest>() == null)
            return;

        feetInside = true;

        if (hasGrown)
        {
            ForceStopFade();
            ShiftToCollideColor();

            if (gardenSpot != null)
                gardenSpot.SetSparkleActive(true);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerFeet"))
            return;

        LunaRest rest = other.GetComponentInParent<LunaRest>();
        if (rest == null)
            return;

        feetInside = true;

        if (hasGrown || isGrowing)
            return;

        if (rest.isResting && restCoroutine == null)
            restCoroutine = StartCoroutine(WaitAndGrow(rest));
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerFeet"))
            return;

        if (other.GetComponentInParent<LunaRest>() == null)
            return;

        feetInside = false;
        StopRestCoroutine();

        if (hasGrown)
        {
            ForceStopFade();
            ApplyGrownIdle();

            if (gardenSpot != null)
                gardenSpot.SetSparkleActive(false);
        }
    }

    private IEnumerator WaitAndGrow(LunaRest rest)
    {
        float elapsed = 0f;

        while (elapsed < restDuration)
        {
            if (rest == null || !rest.isResting || !feetInside)
            {
                restCoroutine = null;
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        restCoroutine = null;

        if (onlyGrowOnce && hasGrown)
            yield break;

        Grow();
    }

    private void Grow()
    {
        hasGrown = true;
        isGrowing = true;

        SaveGrowthState(true);

        if (grassObject != null)
            grassObject.SetActive(true);

        if (gardenSpot != null)
        {
            gardenSpot.Reveal();
            gardenSpot.SetSparkleActive(feetInside);
        }

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeIn());

        if (growthSFX != null)
            StartCoroutine(PlayGrowthSound());
    }

    private IEnumerator FadeIn()
    {
        if (grassRenderer == null)
        {
            isGrowing = false;
            fadeCoroutine = null;
            yield break;
        }

        Color target = feetInside ? grownCollideColor : grownIdleColor;
        target.a = 1f;

        Color start = target;
        start.a = 0f;

        grassRenderer.color = start;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            Color c = Color.Lerp(start, target, t);
            c.a = t;
            grassRenderer.color = c;

            yield return null;
        }

        grassRenderer.color = target;
        isGrowing = false;
        fadeCoroutine = null;
    }

    private void ShiftToCollideColor()
    {
        StartColorShift(grownCollideColor);
    }

    private void ShiftToIdleColor()
    {
        StartColorShift(grownIdleColor);
    }

    private void StartColorShift(Color target)
    {
        if (!hasGrown || grassRenderer == null || isGrowing)
            return;

        target.a = 1f;

        if (colorShiftCoroutine != null)
            StopCoroutine(colorShiftCoroutine);

        colorShiftCoroutine = StartCoroutine(ColorShift(target));
    }

    private IEnumerator ColorShift(Color target)
    {
        Color start = grassRenderer.color;
        start.a = 1f;
        target.a = 1f;

        float elapsed = 0f;

        while (elapsed < colorShiftDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / colorShiftDuration);

            Color c = Color.Lerp(start, target, t);
            c.a = 1f;
            grassRenderer.color = c;

            yield return null;
        }

        grassRenderer.color = target;
        colorShiftCoroutine = null;
    }

    private IEnumerator PlayGrowthSound()
    {
        if (soundDelay > 0f)
            yield return new WaitForSeconds(soundDelay);

        if (audioSource != null && growthSFX != null)
            audioSource.PlayOneShot(growthSFX, growthVolume);
    }

    private void ForceStopFade()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        isGrowing = false;
    }

    private void ApplyGrownVisualImmediate()
    {
        if (grassObject != null)
            grassObject.SetActive(true);

        ApplyGrownIdle();
    }

    private void ApplyGrownIdle()
    {
        if (grassRenderer != null)
        {
            Color c = grownIdleColor;
            c.a = 1f;
            grassRenderer.color = c;
        }
    }

    private void ApplyHiddenVisual()
    {
        if (grassObject != null)
            grassObject.SetActive(false);

        if (grassRenderer != null)
        {
            Color c = grownIdleColor;
            c.a = 0f;
            grassRenderer.color = c;
        }
    }

    private void StopRestCoroutine()
    {
        if (restCoroutine != null)
        {
            StopCoroutine(restCoroutine);
            restCoroutine = null;
        }
    }

    private bool LoadGrowthState()
    {
        if (PlayerPrefs.HasKey(resolvedSaveKey))
            return PlayerPrefs.GetInt(resolvedSaveKey) == 1;

        return startGrown;
    }

    private void SaveGrowthState(bool grown)
    {
        PlayerPrefs.SetInt(resolvedSaveKey, grown ? 1 : 0);
        PlayerPrefs.Save();
    }

    private string BuildResolvedSaveKey()
    {
        string baseKey = string.IsNullOrWhiteSpace(saveKey) ? transform.name : saveKey;

        if (includeSceneNameInKey)
            return SceneManager.GetActiveScene().name + "_" + baseKey;

        return baseKey;
    }
}