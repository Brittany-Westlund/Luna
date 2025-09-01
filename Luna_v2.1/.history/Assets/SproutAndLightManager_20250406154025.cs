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

    [Header("Spore Hint Icon System")]
    public GameObject sporeHintPrefab;
    public Vector3 hintOffsetStage1 = new Vector3(0, 1.5f, 0);
    public Vector3 hintOffsetStage2 = new Vector3(0, 1.2f, 0);
    public Vector3 hintOffsetStage3 = new Vector3(0, 1.0f, 0);
    public float hintScaleStage1 = 1f;
    public float hintScaleStage2 = 0.85f;
    public float hintScaleStage3 = 0.7f;

    [HideInInspector] public bool isPlayerNearby = false;

    private bool isFullyGrown = false;
    private float squaredActivationRange;
    private GameObject currentSporeHint;
    private bool sporeHintActive = false;
    private Coroutine hintDelayCoroutine;

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
        if (sporeHintPrefab == null) return;

        Vector3 selectedOffset = hintOffsetStage3;
        float selectedScale = hintScaleStage3;
        float scale = transform.localScale.x;

        if (scale < 0.10f)
        {
            selectedOffset = hintOffsetStage1;
            selectedScale = hintScaleStage1;
        }
        else if (scale >= 0.10f && scale < 0.15f)
        {
            selectedOffset = hintOffsetStage2;
            selectedScale = hintScaleStage2;
        }
        else if (scale >= 0.15f && scale < maxScale)
        {
            selectedOffset = hintOffsetStage3;
            selectedScale = hintScaleStage3;
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