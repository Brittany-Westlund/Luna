using UnityEngine;
using UnityEngine.UI;  // For Image component
using System.Collections;
using MoreMountains.Tools;

public class LunaHealthWarning : MonoBehaviour
{
    public MMProgressBar healthBar;               // Reference to Luna's health bar
    public Transform avatarHead;                  // Transform for the AvatarHead GameObject
    public AudioSource warningAudio;              // AudioSource for the warning sound effect
    public float scaleAmount = 1.2f;              // Amount to scale the AvatarHead by (e.g., 1.2 for 120% size)
    public float scaleDuration = 0.2f;            // Duration of each scale up/down (seconds)
    public int scaleRepeats = 2;                  // Number of times to scale up/down each trigger
    public float warningRepeatInterval = 2.5f;    // Interval in seconds to repeat the warning
    public Color warningColor = Color.red;        // Color to turn the AvatarHead when scaling
    public float activationDelay = 5f;            // Delay in seconds before activating the script

    private bool warningActive = false;           // Track if the warning coroutine is active
    private bool isActivated = false;             // Tracks if script has fully activated
    private Image avatarImage;                    // Reference to the AvatarHead's Image component
    private Color originalColor;                  // Store the original color of AvatarHead
    private Vector3 originalScale;                // Store the original scale of AvatarHead
    private float previousHealth = 1.0f;          // Track previous health to detect changes

    private void Start()
    {
        StartCoroutine(ActivateAfterDelay()); // Start coroutine to enable script after delay

        // Ensure warning audio doesn’t play on awake and doesn't loop
        if (warningAudio != null)
        {
            warningAudio.playOnAwake = false;
            warningAudio.loop = false;  // Disable looping so we can control playback
            warningAudio.Stop();
        }
    }

    private IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);

        // Initialize avatar visuals
        avatarImage = avatarHead.GetComponent<Image>();
        if (avatarImage != null)
        {
            originalColor = avatarImage.color;  // Save the original color
        }
        originalScale = avatarHead.localScale; // Store original scale

        // Set script as fully activated
        isActivated = true;
        previousHealth = healthBar.BarProgress; // Set previousHealth after delay
        Debug.Log("Script activated after delay; monitoring health changes now.");
    }

    private void Update()
    {
        // Only proceed if the script has fully activated after delay
        if (!isActivated) return;

        float currentHealth = healthBar.BarProgress;

        // Only start the warning if health has just dropped below 20%
        if (currentHealth <= 0.2f && previousHealth > 0.2f && !warningActive)
        {
            StartCoroutine(WarningRoutine());
            warningActive = true;
            Debug.Log("Warning routine started.");
        }
        // Stop the warning if health goes back above 20%
        else if (currentHealth > 0.2f && warningActive)
        {
            warningActive = false;
            StopAllCoroutines();
            if (warningAudio != null && warningAudio.isPlaying) warningAudio.Stop();
            ResetAvatarVisuals();  // Ensure visuals reset when warning stops
            Debug.Log("Warning routine stopped, visuals reset.");
        }

        // Update previous health to the current value for the next frame
        previousHealth = currentHealth;
    }

    private IEnumerator WarningRoutine()
    {
        while (healthBar.BarProgress <= 0.2f)
        {
            // Play the scaling animation and sound effect
            StartCoroutine(ScaleAvatarHead());

            if (warningAudio != null)
            {
                warningAudio.Play();
            }

            // Wait for the specified repeat interval before triggering again
            yield return new WaitForSeconds(warningRepeatInterval);
        }
    }

    private IEnumerator ScaleAvatarHead()
    {
        // Ensure the avatar starts at its original scale
        avatarHead.localScale = originalScale;

        // Set the avatar color to the warning color
        if (avatarImage != null)
        {
            avatarImage.color = warningColor;
        }

        for (int i = 0; i < scaleRepeats; i++)
        {
            // Scale up
            yield return ScaleTo(originalScale * scaleAmount);

            // Scale back to original size
            yield return ScaleTo(originalScale);
        }

        // Reset the avatar color back to its original color
        ResetAvatarVisuals();
    }

    private IEnumerator ScaleTo(Vector3 targetScale)
    {
        Vector3 startScale = avatarHead.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < scaleDuration)
        {
            // Interpolate between the start and target scale over scaleDuration
            avatarHead.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / scaleDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        avatarHead.localScale = targetScale;
    }

    private void ResetAvatarVisuals()
    {
        // Reset the avatar color and scale
        if (avatarImage != null)
        {
            avatarImage.color = originalColor;
        }
        avatarHead.localScale = originalScale; // Ensure it resets to original scale
    }
}
