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

    [HideInInspector] public bool isPlayerNearby = false;

    private bool isFullyGrown = false;
    private float squaredActivationRange;
    private Coroutine iconDelayRoutine;

    void Start()
    {
        squaredActivationRange = ActivationRange * ActivationRange;

        if (LitFlowerRenderer != null) LitFlowerRenderer.enabled = false;
        if (flowerSway != null) flowerSway.enabled = false;
        if (PointsText != null) PointsText.enabled = false;

        DisableAllSporeIcons();
    }

    void Update()
    {
        if (isPlayerNearby && !isFullyGrown)
        {
            if (iconDelayRoutine == null)
            {
                iconDelayRoutine = StartCoroutine(DelayedIconSwitch());
            }
        }
        else
        {
            DisableAllSporeIcons();
            if (iconDelayRoutine != null)
            {
                StopCoroutine(iconDelayRoutine);
                iconDelayRoutine = null;
            }
        }

        if (Input.GetButtonDown("Player1_LightActivation") && isFullyGrown && isPlayerNearby)
        {
            ActivateLight();
        }
    }

    private IEnumerator DelayedIconSwitch()
    {
        yield return new WaitForSeconds(0.2f); // brief delay
        UpdateSporeHintIcons();
        iconDelayRoutine = null;
    }

    private void UpdateSporeHintIcons()
    {
        float scale = transform.localScale.x;

        sporeHintIcon1.SetActive(scale < 0.10f);
        sporeHintIcon2.SetActive(scale >= 0.10f && scale < 0.15f);
        sporeHintIcon3.SetActive(scale >= 0.15f && scale < maxScale);
    }

    private void DisableAllSporeIcons()
    {
        if (sporeHintIcon1 != null) sporeHintIcon1.SetActive(false);
        if (sporeHintIcon2 != null) sporeHintIcon2.SetActive(false);
        if (sporeHintIcon3 != null) sporeHintIcon3.SetActive(false);
    }

    public void ResetOnGrowth()
    {
        if (!isFullyGrown) Grow();

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

        if (flowerSway != null) flowerSway.enabled = true;

        // Notify LightHintController if present
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

            if (LitFlowerRenderer != null) LitFlowerRenderer.enabled = true;
            if (PointsText != null) PointsText.enabled = true;

            ScoreManager.Instance.AddPoint();
            Debug.Log("Light activated and points awarded!");

            // Disable the light icon
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

            if (flowerSway != null) flowerSway.enabled = true;
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
