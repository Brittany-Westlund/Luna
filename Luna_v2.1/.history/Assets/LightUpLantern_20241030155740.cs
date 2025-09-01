using UnityEngine;
using MoreMountains.Tools; // Assuming LightBar uses this namespace

public class LanternLight : MonoBehaviour
{
    public SpriteRenderer LitLanternSprite;    // The lit version of the lantern sprite
    public KeyCode activationKey = KeyCode.L;  // Public field to set the activation key
    public MMProgressBar lightBar;             // Public field to assign the light bar in the Inspector
    public GameObject hiddenObject;            // Reference to the HiddenObject GameObject
    private float lightCost = 0.1f;            // Light cost to light up the lantern
    private bool isLit = false;                // Track whether the lantern is currently lit
    private bool playerInRange = false;        // Track if the player is in range for activation

    private SpriteRenderer objectSpriteRenderer;
    private EdgeCollider2D objectEdgeCollider;

    void Start()
    {
        // Get references to HiddenObject's components
        if (hiddenObject != null)
        {
            objectSpriteRenderer = hiddenObject.GetComponent<SpriteRenderer>();
            objectEdgeCollider = hiddenObject.GetComponent<EdgeCollider2D>();

            // Ensure HiddenObject is initially hidden
            if (objectSpriteRenderer != null) objectSpriteRenderer.enabled = false;
            if (objectEdgeCollider != null) objectEdgeCollider.enabled = false;
        }
    }

    void Update()
    {
        // Check for player input to toggle lantern light
        if (playerInRange && Input.GetKeyDown(activationKey))
        {
            float currentProgress = lightBar.BarProgress;  // Get the current progress (0 to 1 scale)

            if (!isLit && currentProgress >= lightCost)
            {
                // Light up the lantern
                lightBar.SetBar01(currentProgress - lightCost);  // Reduce progress by lightCost
                if (LitLanternSprite != null) LitLanternSprite.enabled = true;
                ToggleHiddenObject(true);
                isLit = true;
            }
            else if (isLit)
            {
                // Retrieve light from the lantern
                lightBar.SetBar01(currentProgress + lightCost);  // Increase progress by lightCost
                if (LitLanternSprite != null) LitLanternSprite.enabled = false;
                ToggleHiddenObject(false);
                isLit = false;
            }
        }
    }

    private void ToggleHiddenObject(bool state)
    {
        // Toggle the SpriteRenderer and EdgeCollider2D on HiddenObject
        if (objectSpriteRenderer != null) objectSpriteRenderer.enabled = state;
        if (objectEdgeCollider != null) objectEdgeCollider.enabled = state;
    }

    // Detect when the player enters the trigger area
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Ensure the player has the tag "Player"
        {
            playerInRange = true;
        }
    }

    // Detect when the player exits the trigger area
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
