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
    public float ActivationRange = 3f;
    public float lightActivationCost = 0.1f;

    [Header("Spore Hint Icon System")]
    public GameObject sporeHintPrefab;
    public Vector3 hintOffsetStage1 = new Vector3(0, 1.5f, 0);
    public Vector3 hintOffsetStage2 = new Vector3(0, 1.1f, 0);
    public Vector3 hintOffsetStage3 = new Vector3(0, 0.85f, 0);
    public float hintScaleStage1 = 1f;
    public float hintScaleStage2 = 0.75f;
    public float hintScaleStage3 = 0.6f;

    [Header("Light Hint Icon System")]
    public GameObject lightHintPrefab;
    public Vector3 lightHintOffset = new Vector3(0, 0.2f, 0);
    public float lightHintScale = 0.6f;

    [HideInInspector] public bool isPlayerNearby = false;
    [HideInInspector] public bool isHeld = false;

    private MMProgressBar LightBar;
    private Text PointsText;
    private SpriteRenderer LitFlowerRenderer;
    private Transform PlayerTransform;
    private FlowerSway flowerSway;

    private bool isFullyGrown = false;
    private GameObject currentSporeHint;
    private GameObject currentLightHint;
    private Coroutine hintDelayCoroutine;

    void Start()
    {
        if (LightBar == null)
        {
            GameObject lb = GameObject.FindWithTag("LightBar");
            if (lb != null) LightBar = lb.GetComponent<MMProgressBar>();
        }

        if (PointsText == null && ScoreManager.Instance != null)
        {
            PointsText = ScoreManager.Instance.PointsText;
        }

        if (LitFlowerRenderer == null)
        {
            Transform litChild = FindDeepChild(transform, "LitFlowerB");
            if (litChild != null) LitFlowerRenderer = litChild.GetComponent<SpriteRenderer>();
        }

        flowerSway = GetComponent<FlowerSway>();

        if (LitFlowerRenderer != null) LitFlowerRenderer.enabled = false;
        if (flowerSway != null) flowerSway.enabled = false;
        if (PointsText != null) PointsText.enabled = false;
    }

    void Update()
    {
        if (isHeld) return;

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

    private IEnumerator ShowSporeHintWithDelay()
    {
        yield return new WaitForSeconds(0.1f);
        ShowSporeHintBasedOnScale();
        hintDelayCoroutine = null;
    }

    private void ShowSporeHintBasedOnScale()
    {
        if (sporeHintPrefab == null || isHeld) return;

        float scale = transform.localScale.x;
        Vector3 selectedOffset;
        float selectedScale;

        if (scale < 0.1f)
        {
            selectedOffset = hintOffsetStage1;
            selectedScale = hintScaleStage1;
        }
        else if (scale < 0.2f)
        {
            selectedOffset = hintOffsetStage2;
            selectedScale = hintScaleStage2;
        }
        else if (scale < maxScale)
        {
            selectedOffset = hintOffsetStage3;
            selectedScale = hintScaleStage3;
        }
        else
        {
            HideSporeHint();
            return;
        }

        if (currentSporeHint == null)
        {
            currentSporeHint = Instantiate(sporeHintPrefab, transform.position + selectedOffset, Quaternion.identity, transform);
            currentSporeHint.transform.localScale = Vector3.one * selectedScale;
        }
        else
        {
            currentSporeHint.transform.localPosition = selectedOffset;
            currentSporeHint.transform.localScale = Vector3.one * selectedScale;
        }
    }

    private void HideSporeHint()
    {
        if (currentSporeHint != null)
        {
            Destroy(currentSporeHint);
            currentSporeHint = null;
        }

        if (hintDelayCoroutine != null)
        {
            StopCoroutine(hintDelayCoroutine);
            hintDelayCoroutine = null;
        }
    }

    public void ResetOnGrowth()
    {
        if (isFullyGrown || isHeld) return;

        float predictedScale = transform.localScale.x + growthIncrement;

        if (predictedScale >= maxScale)
        {
            StartCoroutine(SmoothGrowToMax());
        }
        else
        {
            StartCoroutine(SmoothGrow());
        }
    }

    private IEnumerator SmoothGrow()
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3(Mathf.Min(transform.localScale.x + growthIncrement, maxScale), Mathf.Min(transform.localScale.y + growthIncrement, maxScale), 1f);
        Vector3 startPosition = transform.position;
        Vector3 endPosition = new Vector3(transform.position.x, Mathf.Min(transform.position.y + yPositionIncrement, maxHeight), transform.position.z);

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

        yield return new WaitForSeconds(0.05f);
        if (!isFullyGrown && isPlayerNearby)
        {
            ShowSporeHintBasedOnScale();
        }
    }

    private IEnumerator SmoothGrowToMax()
    {
        Vector3 startScale = transform.localScale;
        Vector3 endScale = new Vector3(maxScale, maxScale, 1f);
        Vector3 startPosition = transform.position;
        Vector3 endPosition = new Vector3(transform.position.x, Mathf.Min(transform.position.y + yPositionIncrement, maxHeight), transform.position.z);

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

        FullyGrow();
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
        if (LightBar != null && ScoreManager.Instance != null && LightBar.BarProgress >= lightActivationCost)
        {
            LightBar.UpdateBar01(LightBar.BarProgress - lightActivationCost);

            if (LitFlowerRenderer != null) LitFlowerRenderer.enabled = true;
            if (PointsText != null) PointsText.enabled = true;

            ScoreManager.Instance.AddPoint();

            if (currentLightHint != null)
            {
                Destroy(currentLightHint);
                currentLightHint = null;
            }
        }
        else
        {
            Debug.Log("Not enough light energy or missing references.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (!isHeld && flowerSway != null) flowerSway.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (!isHeld && !isFullyGrown && flowerSway != null) flowerSway.enabled = false;
        }
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform found = FindDeepChild(child, name);
            if (found != null) return found;
        }
        return null;
    }
}
