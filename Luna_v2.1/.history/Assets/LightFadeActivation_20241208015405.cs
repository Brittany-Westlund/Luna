using UnityEngine;

public class LightFadeActivation : MonoBehaviour
{
    public SpriteRenderer flowerRenderer; // Reference to the flower's SpriteRenderer
    public Transform playerTransform; // Reference to the player's transform
    public float activationRange = 3f; // The range within which the flower can be activated
    public float fadeSpeed = 1f; // The speed at which the flower fades in

    private Color targetColor = Color.white; // The target color for the flower
    private float squaredActivationRange; // Cached squared range for optimization
    private bool isFading = false; // Flag to track if the flower is currently fading

    void Start()
    {
        // Precompute the squared activation range for optimization
        squaredActivationRange = activationRange * activationRange;
    }

    void Update()
    {
        // Check if the player presses the aid/assist button and is within range
        if (Input.GetButtonDown("Aid") && IsPlayerInRange())
        {
            // Start fading in the flower
            isFading = true;
        }

        // Fade in the flower if it's currently fading
        if (isFading)
        {
            FadeInFlower();
        }
    }

    private bool IsPlayerInRange()
    {
        // Check the squared distance between this flower and the player for optimized performance
        float squaredDistanceToPlayer = (playerTransform.position - transform.position).sqrMagnitude;
        return squaredDistanceToPlayer <= squaredActivationRange;
    }

    private void FadeInFlower()
    {
        // Calculate the new alpha value based on the fade speed
        float newAlpha = Mathf.MoveTowards(flowerRenderer.color.a, targetColor.a, fadeSpeed * Time.deltaTime);
        // Update the flower's color with the new alpha value
        flowerRenderer.color = new Color(flowerRenderer.color.r, flowerRenderer.color.g, flowerRenderer.color.b, newAlpha);

        // Stop fading if the flower has fully faded in
        if (flowerRenderer.color.a >= targetColor.a)
        {
            isFading = false;
        }
    }
}
