using UnityEngine;
using System.Collections;

public class LunaDeathHandler : MonoBehaviour
{
    public AudioSource deathSound;                 // Sound to play when Luna's health reaches zero
    public LunaHealthWarning healthWarningScript;  // Reference to the LunaHealthWarning script
    public MoreMountains.Tools.MMProgressBar healthBar; // Reference to Luna's health bar
    public MoreMountains.CorgiEngine.CorgiController movementController; // Reference to her movement controller
    public AudioSource backgroundMusic;            // Reference to the background music AudioSource
    public float slowMovementSpeed = 2f;           // New slow speed for horizontal movement
    public float ledgeDisableDelay = 2f;           // Delay before ledges disappear
    public float fadeOutDuration = 2f;             // Duration for the background music fade-out
    private bool isDead = false;                   // Tracks if Luna has already "died"

    private void Update()
    {
        // Check if Luna's health has reached zero
        if (healthBar.BarProgress <= 0 && !isDead)
        {
            isDead = true;  // Mark Luna as "dead" to prevent repeating actions
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        // Play and loop the death sound
        if (deathSound != null)
        {
            deathSound.loop = true;
            deathSound.Play();
        }

        // Start fading out the background music
        if (backgroundMusic != null)
        {
            StartCoroutine(FadeOutBackgroundMusic());
        }

        // Stop the LunaHealthWarning script
        if (healthWarningScript != null) healthWarningScript.enabled = false;

        // Disable the light controls by blocking keys B and F
        DisableLightControls();

        // Slow down the horizontal movement speed by adjusting WalkSpeed
        var horizontalMovement = movementController.GetComponent<MoreMountains.CorgiEngine.CharacterHorizontalMovement>();
        if (horizontalMovement != null)
        {
            horizontalMovement.WalkSpeed = slowMovementSpeed;
        }
        else
        {
            Debug.LogWarning("CharacterHorizontalMovement component not found. Unable to adjust WalkSpeed.");
        }
    }

    private void DisableLightControls()
    {
        // Prevent actions bound to B and F keys
        // Optionally, integrate this with your existing light controls to block light usage
        // A simple approach is to check for this "isDead" state in your light controls script
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Detect collision with ledges when Luna is "dead"
        if (isDead && other.CompareTag("Ledge"))
        {
            StartCoroutine(DisableLedgeAfterDelay(other.gameObject));
        }
    }

    private IEnumerator DisableLedgeAfterDelay(GameObject ledge)
    {
        yield return new WaitForSeconds(ledgeDisableDelay);
        ledge.SetActive(false); // Disable the ledge to make Luna fall
    }

    private IEnumerator FadeOutBackgroundMusic()
    {
        float startVolume = backgroundMusic.volume;

        while (backgroundMusic.volume > 0)
        {
            backgroundMusic.volume -= startVolume * Time.deltaTime / fadeOutDuration;
            yield return null;
        }

        backgroundMusic.Stop();
        backgroundMusic.volume = startVolume; // Reset volume for future use
    }
}
