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

    [Header("UI Feedback")]
    public GameObject SporeIcon1;
    public GameObject SporeIcon2;
    public GameObject SporeIcon3;

    [HideInInspector] public bool isPlayerNearby = false;

    private bool isFullyGrown = false;
    private float squaredActivationRange;

    void Start()
    {
        squaredActivationRange = ActivationRange * ActivationRange;

        if (LitFlowerRenderer != null) LitFlowerRenderer.enabled = false;
        if (flowerSway != null) flowerSway.enabled = false;
        if (PointsText != null) PointsText.enabled = false;

        // Start with all icons off
        SetAllSporeIcons(false);
    }

    void Update()
    {
        if (Input.GetButtonDown("Player1_LightActivation") && isFullyGrown && isPlayerNearby)
        {
            ActivateLight();
        }

        UpdateSporeIconVisibility();
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
        if (flowerSway != null) flowerSway.enabled = true;
        SetAllSporeIcons(false); // Hide spore hints once fully grown
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

    private void SetAllSporeIcons(bool state)
    {
        if (SporeIcon1 != null) SporeIcon1.SetActive(state);
        if (SporeIcon2 != null) SporeIcon2.SetActive(state);
        if (SporeIcon3 != null) SporeIcon3.SetActive(state);
    }

    private void UpdateSporeIconVisibility()
    {
        if (!isPlayerNearby || isFullyGrown)
        {
            SetAllSporeIcons(false);
            return;
        }

        float scale = transform.localScale.x;

        if (scale < 0.12f) // Very early stage
        {
            SporeIcon1?.SetActive(true);
            SporeIcon2?.SetActive(false);
            SporeIcon3?.SetActive(false);
        }
        else if (scale < 0.22f) // Middle stage
        {
            SporeIcon1?.SetActive(false);
            SporeIcon2?.SetActive(true);
            SporeIcon3?.SetActive(false);
        }
        else // Near full growth
        {
            SporeIcon1?.SetActive(false);
            SporeIcon2?.SetActive(false);
            SporeIcon3?.SetActive(true);
        }
    }
}
