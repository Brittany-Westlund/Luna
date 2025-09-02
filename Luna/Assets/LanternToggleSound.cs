using UnityEngine;

public class LanternToggle : MonoBehaviour
{
    public SpriteRenderer lanternSprite;       // Reference to the Lantern's SpriteRenderer
    public KeyCode activationKey = KeyCode.E;  // Key to toggle the lantern light
    public AudioSource toggleOnSound;         // AudioSource for the toggle-on sound
    public AudioSource toggleOffSound;        // AudioSource for the toggle-off sound
    
    private bool isLit = false;                // Tracks whether the lantern is currently lit

    private void Start()
    {
        // Get both AudioSources attached to this GameObject
        AudioSource[] audioSources = GetComponents<AudioSource>();
        
        if (audioSources.Length >= 2)
        {
            toggleOnSound = audioSources[0];
            toggleOffSound = audioSources[1];
        }
        else
        {
            Debug.LogError("Please add two AudioSource components to the lantern.");
        }
    }

    private void Update()
    {
        // Check for the toggle input
        if (Input.GetKeyDown(activationKey))
        {
            ToggleLantern();
        }
    }

    private void ToggleLantern()
    {
        // Toggle the isLit state
        isLit = !isLit;

        // Update the lantern's visual state and play the appropriate sound
        if (isLit)
        {
            // Turn on the lantern light
            if (lanternSprite != null)
            {
                lanternSprite.enabled = true;
            }

            // Play the toggle-on sound if available
            if (toggleOnSound != null)
            {
                toggleOnSound.Play();
            }
        }
        else
        {
            // Turn off the lantern light
            if (lanternSprite != null)
            {
                lanternSprite.enabled = false;
            }

            // Play the toggle-off sound if available
            if (toggleOffSound != null)
            {
                toggleOffSound.Play();
            }
        }
    }
}
