using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;

public class BloomLightActivation : MonoBehaviour
{
    public Text PointsText; // UI to display points earned
    public MMProgressBar LightBar; // Progress bar for Luna's energy
    public SpriteRenderer LitFlowerARenderer; // Renderer for the lit flower
    public Transform PlayerTransform; // Player's transform
    public float ActivationRange = 3f; // Range to activate the light

    private float lightCost = 0.1f; // Cost of activating the light
    private float squaredActivationRange; // Cached squared range for optimization

    void Start()
    {
        squaredActivationRange = ActivationRange * ActivationRange;

        if (LitFlowerARenderer != null)
        {
            LitFlowerARenderer.enabled = false; // Ensure the lit flower starts disabled
        }
    }

    void Update()
    {
        // Check for input to activate light
        if (Input.GetButtonDown("Player1_LightActivation") && IsPlayerInRange())
        {
            ActivateLight();
        }
    }

    private void ActivateLight()
    {
        if (LightBar.BarProgress >= lightCost)
        {
            LightBar.UpdateBar01(LightBar.BarProgress - lightCost); // Deduct light energy

            if (LitFlowerARenderer != null)
            {
                LitFlowerARenderer.enabled = true; // Show the lit flower
            }

            if (PointsText != null)
            {
                PointsText.enabled = true; // Display points earned
            }

            // Example scoring system (ensure you have a ScoreManager in your game)
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
        // Check the squared distance between the player and the flower
        float squaredDistance = (PlayerTransform.position - transform.position).sqrMagnitude;
        return squaredDistance <= squaredActivationRange;
    }
}
