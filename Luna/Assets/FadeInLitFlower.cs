using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Tools;

public class EnableLitFlower : MonoBehaviour
{
    public SpriteRenderer litFlowerRenderer; // Reference to the LitFlower's SpriteRenderer
    public Text pointsText; // Reference to the points display text
    public MMProgressBar lightBar; // Reference to the light bar
    public float lightCost = 0.1f; // Cost of light to activate the flower

    private bool isActivated = false; // Flag to track if the LitFlower is activated

    void Start()
    {
    
    }

    void Update()
    {
        // Listen for the 'B' button press to activate the LitFlower
        if (Input.GetButtonDown("B") && !isActivated && lightBar.BarProgress >= lightCost)
        {
            ActivateLitFlower();
        }
    }

    public void ActivateLitFlower()
    {
        // Deduct light cost from the light bar
        lightBar.UpdateBar01(lightBar.BarProgress - lightCost);

        // Enable the LitFlower sprite
        if (litFlowerRenderer != null)
        {
            litFlowerRenderer.enabled = true;
        }

        // Enable the points text
        if (pointsText != null)
        {
            pointsText.enabled = true;
        }

        // Set the flag to indicate that the LitFlower is activated
        isActivated = true;
    }
}
