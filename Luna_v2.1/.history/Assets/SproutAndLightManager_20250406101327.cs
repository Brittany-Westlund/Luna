using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;

public class SproutAndLightManager : MonoBehaviour
{
    [Header("Growth Settings")]
    public float growthIncrement = 0.07f;    // Growth amount per aid
    public float yPositionIncrement = 0.04f; // Upward movement per aid
    public float maxScale = 0.3f;            // Maximum size of the sprout
    public float maxHeight = 1.8f;           // Maximum upward position

    [Header("Light Activation Settings")]
    public Text PointsText;                     // UI text for points display
    public MMProgressBar LightBar;              // Progress bar for Luna's energy
    public SpriteRenderer LitFlowerRenderer;    // Renderer for the lit flower
    public Transform PlayerTransform;           // Player's position
    public float ActivationRange = 3f;          // Range to activate the light
    public float lightActivationCost = 0.1f;    // Energy cost for lighting up
    public FlowerSway flowerSway;               // Reference to FlowerSway script
    [Header("UI Feedback")]
    public GameObject sporeHintIcon;
    
    [HideInInspector] public bool isPlayerNearby = false;
    private bool isFullyGrown = false;          // Whether the sprout is fully grown
    private float squaredActivationRange;       // Cached squared range for performance

    private SpriteRenderer sporeHintRenderer;

    void Start()
    {
        if (sporeHintIcon != null)
        {
            sporeHintRenderer = sporeHintIcon.GetComponent<SpriteRenderer>();
            if (sporeHintRenderer != null)
            {
                sporeHintRenderer.enabled = false;
            }
        }
    }


    // Called when Luna aids the sprout
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
            flowerSway.enabled = true; // Enable Flower Sway
        }

        Debug.Log("Sprout is fully grown and ready to be lit!");
    }

 void Update()
    {
        bool shouldShowSporeHint = isPlayerNearby && !isFullyGrown;

      if (sporeHintRenderer != null)
    {
        sporeHintRenderer.enabled = isPlayerNearby && !isFullyGrown;
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
            LightBar.UpdateBar01(LightBar.BarProgress - lightActivationCost); // Deduct light energy

            if (LitFlowerRenderer != null)
            {
                LitFlowerRenderer.enabled = true; // Show the lit flower
            }

            if (PointsText != null)
            {
                PointsText.enabled = true; // Show points awarded
            }

            // Example scoring system (ensure ScoreManager is in game)
            ScoreManager.Instance.AddPoint();
            Debug.Log("Light activated and points awarded!");
        }
        else
        {
            Debug.Log("Not enough light energy to activate!");
        }
    }

    private bool IsPlayerInRange()
    {
        // Check squared distance for performance
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
                flowerSway.enabled = false; // Only sway when player is close & not fully grown
            }
        }
    }
}

