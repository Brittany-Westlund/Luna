using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;

public class SproutAndLightManager : MonoBehaviour
{
    [Header("Growth Settings")]
    public float growthIncrement = 0.07f;
    public float yPositionIncrement = 0.04f;
    public float maxScale = 0.3f;
    public float maxHeight = 1.8f;

    [Header("Light Activation Settings")]
    public Text PointsText;
    public MMProgressBar LightBar;
    public SpriteRenderer LitFlowerRenderer;
    public Transform PlayerTransform;
    public float ActivationRange = 3f;
    public float lightActivationCost = 0.1f;
    public FlowerSway flowerSway;

    [Header("Spore Hint Icons")]
    public GameObject sporeHintIcon1;
    public GameObject sporeHintIcon2;
    public GameObject sporeHintIcon3;
    public float fadeDuration = 0.2f;

    [HideInInspector] public bool isPlayerNearby = false;

    private bool isFullyGrown = false;
    private float squaredActivationRange;

    private SpriteRenderer spore1Renderer;
    private SpriteRenderer spore2Renderer;
    private SpriteRenderer spore3Renderer;
    private Coroutine activeFadeCoroutine = null;
    private SpriteRenderer currentActiveIcon = null;    

    void Start()
    {
        squaredActivationRange = ActivationRange * ActivationRange;

        if (LitFlowerRenderer != null)
        {
            LitFlowerRenderer.enabled = false;
        }

        if (flowerSway != null)
        {
            flowerSway.enabled = false;
        }

        if (PointsText != null)
        {
            PointsText.enabled = false;
        }

        spore1Renderer = sporeHintIcon1?.GetComponent<SpriteRenderer>();
        spore2Renderer = sporeHintIcon2?.GetComponent<SpriteRenderer>();
        spore3Renderer = sporeHintIcon3?.GetComponent<SpriteRenderer>();

        SetAlpha(spore1Renderer, 0f);
        SetAlpha(spore2Renderer, 0f);
        SetAlpha(spore3Renderer, 0f);
    }

    void Update()
    {
        if (isPlayerNearby && !isFullyGrown)
        {
            UpdateSporeHintIconsWithFade();
        }
        else
        {
            FadeAllSporeIconsOut();
        }

        if (Input.GetButtonDown("Player1_LightActivation") && isFullyGrown && isPlayerNearby)
        {
            ActivateLight();
        }
    }

    void UpdateSporeHintIconsWithFade()
{
    if (!isPlayerNearby || isFullyGrown)
    {
        FadeAllSporeIconsOut();
        return;
    }

    float scale = transform.localScale.x;
    SpriteRenderer targetIcon = null;

    if (scale < 0.10f)
        targetIcon = spore1Renderer;
    else if (scale >= 0.10f && scale < 0.15f)
        targetIcon = spore2Renderer;
    else if (scale >= 0.15f && scale < maxScale)
        targetIcon = spore3Renderer;

    if (targetIcon != currentActiveIcon)
    {
        if (activeFadeCoroutine != null)
        {
            StopCoroutine(activeFadeCoroutine);
        }

        activeFadeCoroutine = StartCoroutine(DelayedFadeSwitch(targetIcon));
    }
}

IEnumerator DelayedFadeSwitch(SpriteRenderer target)
{
    // Immediately fade out the previous icon
    if (currentActiveIcon != null)
        SetAlpha(currentActiveIcon, 0f);

    yield return new WaitForSeconds(0.15f); // Add a brief delay before fading in the new one

    // Set the new active icon
    currentActiveIcon = target;

    if (currentActiveIcon != null)
        yield return StartCoroutine(FadeSprite(currentActiveIcon, 1f));
}

    void StartFadeExclusive(SpriteRenderer target, float targetAlpha)
    {
        // Instantly hide all icons except the target
        if (spore1Renderer != null && spore1Renderer != target)
        {
            StopAllCoroutines();
            SetAlpha(spore1Renderer, 0f);
        }
        if (spore2Renderer != null && spore2Renderer != target)
        {
            StopAllCoroutines();
            SetAlpha(spore2Renderer, 0f);
        }
        if (spore3Renderer != null && spore3Renderer != target)
        {
            StopAllCoroutines();
            SetAlpha(spore3Renderer, 0f);
        }

        // Fade in only the correct one
        if (target != null && target.color.a < 0.95f) // only fade if not already fully visible
        {
            StartCoroutine(FadeSprite(target, targetAlpha));
        }
    }



    void FadeAllSporeIconsOut()
    {
        StartFade(spore1Renderer, 0f);
        StartFade(spore2Renderer, 0f);
        StartFade(spore3Renderer, 0f);
    }

    void StartFade(SpriteRenderer renderer, float targetAlpha)
    {
        if (renderer != null)
        {
            StartCoroutine(FadeSprite(renderer, targetAlpha));
        }
    }

    IEnumerator FadeSprite(SpriteRenderer renderer, float targetAlpha)
    {
        float startAlpha = renderer.color.a;
        float timeElapsed = 0f;

        while (timeElapsed < fadeDuration)
        {
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / fadeDuration);
            SetAlpha(renderer, newAlpha);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        SetAlpha(renderer, targetAlpha);
    }

    void SetAlpha(SpriteRenderer renderer, float alpha)
    {
        if (renderer != null)
        {
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }

    public void ResetOnGrowth()
    {
        if (!isFullyGrown)
        {
            Grow();
        }

        if (transform.localScale.x >= maxScale && !isFullyGrown)
        {
            FullyGrow();
        }
    }

    private void Grow()
    {
        if (transform.localScale.x + growthIncrement <= maxScale &&
            transform.localScale.y + growthIncrement <= maxScale)
        {
            transform.localScale += new Vector3(growthIncrement, growthIncrement, 0);
        }
        else
        {
            transform.localScale = new Vector3(maxScale, maxScale, 1);
        }

        if (transform.position.y + yPositionIncrement <= maxHeight)
        {
            transform.position += new Vector3(0, yPositionIncrement, 0);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, maxHeight, transform.position.z);
        }
    }

    private void FullyGrow()
    {
        isFullyGrown = true;

        if (flowerSway != null)
        {
            flowerSway.enabled = true;
        }

        LightHintController lightHint = GetComponent<LightHintController>();
        if (lightHint != null)
        {
            lightHint.SetFullyGrown(true);
        }

        Debug.Log("Sprout is fully grown and ready to be lit!");
    }

    private void ActivateLight()
    {
        if (LightBar.BarProgress >= lightActivationCost)
        {
            LightBar.UpdateBar01(LightBar.BarProgress - lightActivationCost);

            if (LitFlowerRenderer != null)
            {
                LitFlowerRenderer.enabled = true;
            }

            if (PointsText != null)
            {
                PointsText.enabled = true;
            }

            ScoreManager.Instance.AddPoint();
            Debug.Log("Light activated and points awarded!");

            LightHintController lightHint = GetComponent<LightHintController>();
            if (lightHint != null)
            {
                lightHint.Deactivate();
            }
        }
        else
        {
            Debug.Log("Not enough light energy to activate!");
        }
    }

    private bool IsPlayerInRange()
    {
        float squaredDistance = (PlayerTransform.position - transform.position).sqrMagnitude;
        return squaredDistance <= squaredActivationRange;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;

            if (flowerSway != null)
            {
                flowerSway.enabled = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;

            if (!isFullyGrown && flowerSway != null)
            {
                flowerSway.enabled = false;
            }
        }
    }
}
