using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;

public class SproutAndLightManager : MonoBehaviour
{
    [Header("Growth Settings")]
    public float growthIncrement = 0.1f;
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

    [Header("Spore Hint Icon System")]
    public GameObject sporeHintPrefab;
    public Vector3 hintOffsetStage1 = new Vector3(0, 1.5f, 0);
    public Vector3 hintOffsetStage2 = new Vector3(0, 1.2f, 0);
    public float hintScaleStage1 = 1f;
    public float hintScaleStage2 = 0.85f;

    [Header("Light Hint Icon System")]
    public GameObject lightHintPrefab;
    public Vector3 lightHintOffset = new Vector3(0, 1.1f, 0);
    public float lightHintScale = 0.6f;

    [HideInInspector] public bool isPlayerNearby = false;

    private bool isFullyGrown = false;
    private float squaredActivationRange;
    private GameObject currentSporeHint;
    private GameObject currentLightHint;
    private bool sporeHintActive = false;
    private Coroutine hintDelayCoroutine;

    void Start()
    {
        squaredActivationRange = ActivationRange * ActivationRange;

        if (LitFlowerRenderer != null) LitFlowerRenderer.enabled = false;
        if (flowerSway != null) flowerSway.enabled = false;
        if (PointsText != null) PointsText.enabled = false;
    }

    void Update()
    {
        if (isPlayerNearby && !isFullyGrown)
        {
            if (hintDelayCoroutine == null)
            {
                hintDelayCoroutine = StartCoroutine(ShowSporeHintWithDelay());
            }
        }
        else
        {
            HideSporeHint();
        }

        if (Input.GetButtonDown("Player1_LightActivation") && isFullyGrown && isPlayerNearby)
        {
            ActivateLight();
        }
    }

    IEnumerator ShowSporeHintWithDelay()
    {
        yield return new WaitForSeconds(0.1f);
        ShowSporeHintBasedOnScale();
        hintDelayCoroutine = null;
    }

    void ShowSporeHintBasedOnScale()
    {
        if (sporeHintPrefab == null || isFullyGrown) return;

        Vector3 selectedOffset = hintOffsetStage2;
        float selectedScale = hintScaleStage2;
        float scale = transform.localScale.x;

        if (scale < 0.15f)
        {
            selectedOffset = hintOffsetStage1;
            selectedScale = hintScaleStage1;
        }
        else if (scale >= 0.15f && scale < maxScale)
        {
            selectedOffset = hintOffsetStage2;
            selectedScale = hintScaleStage2;
        }
        else
        {
            HideSporeHint();
            return;
        }

        if (!sporeHintActive)
        {
            currentSporeHint = Instantiate(sporeHintPrefab, transform.position + selectedOffset, Quaternion.identity, transform);
            currentSporeHint.transform.localScale = Vector3.one * selectedScale;
            sporeHintActive = true;
        }
        else if (currentSporeHint != null)
        {
            currentSporeHint.transform.localPosition = selectedOffset;
            currentSporeHint.transform.localScale = Vector3.one * selectedScale;
        }
    }

    void HideSporeHint()
    {
        if (sporeHintActive && currentSporeHint != null)
        {
            Destroy(currentSporeHint);
            currentSporeHint = null;
            sporeHintActive = false;
        }

        if (hintDelayCoroutine != null)
        {
            StopCoroutine(hintDelayCoroutine);
            hintDelayCoroutine = null;
        }
    }

    public void ResetOnGrowth()
    {
        if (!isFullyGrown)
        {
            StartCoroutine(SmoothGrow());
        }
    }

    private IEnumerator SmoothGrow()
    {
        float targetScale = Mathf.Min(transform.localScale.x + growthIncrement, maxScale);
        float targetHeight = Mathf.Min(transform.position.y + yPositionIncrement, maxHeight);
        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3(targetScale, targetScale, 1);
        Vector3 startPosition = transform.position;
        Vector3 endPosition = new Vector3(transform.position.x, targetHeight, transform.position.z);

        float elapsed = 0f;
        float duration = 0.15f;

        HideSporeHint();

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = endScale;
        transform.position = endPosition;

        if (transform.localScale.x >= maxScale && !isFullyGrown)
        {
            FullyGrow();
        }
        else if (isPlayerNearby)
        {
            ShowSporeHintBasedOnScale();
        }
    }

    private void FullyGrow()
    {
        isFullyGrown = true;

        if (flowerSway != null) flowerSway.enabled = true;

        if (lightHintPrefab != null)
        {
            currentLightHint = Instantiate(lightHintPrefab, transform.position + lightHintOffset, Quaternion.identity, transform);
            currentLightHint.transform.localScale = Vector3.one * lightHintScale;
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

            if (currentLightHint != null)
            {
                Destroy(currentLightHint);
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
            if (flowerSway != null) flowerSway.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (!isFullyGrown && flowerSway != null) flowerSway.enabled = false;
        }
    }
}
