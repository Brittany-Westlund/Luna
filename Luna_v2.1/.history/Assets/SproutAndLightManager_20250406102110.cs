using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;
using System.Collections.Generic;


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

    [Header("UI Feedback")]
    public List<SpriteRenderer> sporeHintRenderers; // Assign all the icons here in Inspector

    [HideInInspector] public bool isPlayerNearby = false;
    private bool isFullyGrown = false;
    private float squaredActivationRange;

    void Start()
    {
        squaredActivationRange = ActivationRange * ActivationRange;

        if (LitFlowerRenderer != null) LitFlowerRenderer.enabled = false;
        if (flowerSway != null) flowerSway.enabled = false;
        if (PointsText != null) PointsText.enabled = false;

        HideAllSporeHints();
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
        transform.localScale += new Vector3(growthIncrement, growthIncrement, 0);
        transform.localScale = new Vector3(
            Mathf.Min(transform.localScale.x, maxScale),
            Mathf.Min(transform.localScale.y, maxScale),
            1
        );

        float newY = Mathf.Min(transform.position.y + yPositionIncrement, maxHeight);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void FullyGrow()
    {
        isFullyGrown = true;
        if (flowerSway != null) flowerSway.enabled = true;
        HideAllSporeHints();
        Debug.Log("Sprout is fully grown and ready to be lit!");
    }

    void Update()
    {
        if (isPlayerNearby && !isFullyGrown)
        {
            ShowAllSporeHints();
        }
        else
        {
            HideAllSporeHints();
        }

        if (Input.GetButtonDown("Player1_LightActivation") && isFullyGrown && isPlayerNearby)
        {
            ActivateLight();
        }
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
        }
        else
        {
            Debug.Log("Not enough light energy to activate!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (flowerSway != null && !isFullyGrown) flowerSway.enabled = true;
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

    private void ShowAllSporeHints()
    {
        foreach (var icon in sporeHintRenderers)
        {
            if (icon != null) icon.enabled = true;
        }
    }

    private void HideAllSporeHints()
    {
        foreach (var icon in sporeHintRenderers)
        {
            if (icon != null) icon.enabled = false;
        }
    }
}
