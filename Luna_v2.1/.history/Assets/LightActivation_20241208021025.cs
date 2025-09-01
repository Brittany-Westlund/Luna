using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;

public class BloomLightActivation : MonoBehaviour
{
    public Text PointsText;
    public MMProgressBar LightBar;
    public SpriteRenderer LitFlowerARenderer;
    public Transform PlayerTransform; // Reference to the player's transform
    public float ActivationRange = 3f; // The range within which the flower can be activated

    private float lightCost = 0.1f;
    private float squaredActivationRange; // Cached squared range for optimization

    void Start()
    {
        // Precompute the squared activation range for optimization
        squaredActivationRange = ActivationRange * ActivationRange;
    }

    void Update()
    {
        // Check for player input to activate light
        if (Input.GetButtonDown("Player1_LightActivation"))
        {
            if (IsPlayerInRange() && LightBar.BarProgress >= lightCost)
            {
                // Decrease light bar progress
                LightBar.UpdateBar01(LightBar.BarProgress - lightCost);

                // Enable the LitFlowerARenderer
                if (LitFlowerARenderer != null)
                {
                    LitFlowerARenderer.enabled = true;
                    Debug.Log("LitFlowerARenderer enabled!");
                    
                    // Additional Debugging
                    Debug.Log("Sprite: " + LitFlowerARenderer.sprite.name);
                    Debug.Log("Color: " + LitFlowerARenderer.color);
                    Debug.Log("Sorting Layer: " + LitFlowerARenderer.sortingLayerName);
                    Debug.Log("Sorting Order: " + LitFlowerARenderer.sortingOrder);
                    Debug.Log("Transform Position: " + LitFlowerARenderer.transform.position);
                    Debug.Log("Camera Position: " + Camera.main.transform.position);
                    Debug.Log("Is Visible?: " + LitFlowerARenderer.isVisible);
                }
                else
                {
                    Debug.LogError("LitFlowerARenderer is null!");
                }

                // Enable the PointsText
                PointsText.enabled = true;

                // Add points (assuming ScoreManager is a singleton)
                ScoreManager.Instance.AddPoint();
            }
        }
    }

    // Check if the player is within the activation range
    private bool IsPlayerInRange()
    {
        // Check the squared distance between this flower and the player for optimized performance
        float squaredDistanceToPlayer = (PlayerTransform.position - transform.position).sqrMagnitude;
        return squaredDistanceToPlayer <= squaredActivationRange;
    }
}
