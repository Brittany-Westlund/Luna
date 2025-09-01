using UnityEngine;
using System.Collections;
using MoreMountains.CorgiEngine;

public class LunaDeathHandler : MonoBehaviour
{
    public AudioSource deathSound;                // Sound to play when Luna's health reaches zero
    public MoreMountains.Tools.MMProgressBar healthBar;
    public MoreMountains.CorgiEngine.CorgiController movementController;
    public float slowMovementSpeed = 2f;
    public float ledgeDisableDelay = 2f;
    private bool isDead = false;

    private void Start()
    {
        // Ensure death sound doesn’t play on start
        if (deathSound != null)
        {
            deathSound.playOnAwake = false;
            deathSound.loop = true;  // Only set loop if desired
        }
    }

    private void Update()
    {
        // Check if Luna's health has reached zero
        if (healthBar.BarProgress <= 0 && !isDead)
        {
            isDead = true;
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        // Play the looping death sound only once when health reaches zero
        if (deathSound != null && !deathSound.isPlaying)
        {
            deathSound.Play();
        }

        DisableLightControls();

        // Slow down movement by adjusting WalkSpeed
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
        // Logic to disable B and F keys if needed
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

        // Disable CircleCollider2D on the ledge to prevent Luna from holding onto it
        var circleCollider = ledge.GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            circleCollider.enabled = false;
        }

        // Optionally, disable the ledge sprite or other components as needed
        ledge.SetActive(false);
    }
}
