using UnityEngine;
using System.Collections;
using MoreMountains.CorgiEngine;

public class LunaDeathHandler : MonoBehaviour
{
    public AudioSource deathSound;
    public LunaHealthWarning healthWarningScript;
    public MoreMountains.Tools.MMProgressBar healthBar;
    public MoreMountains.CorgiEngine.CorgiController movementController;
    public GameObject musicGameObject; // Reference to the GameObject with BackgroundMusic
    public float slowMovementSpeed = 2f;
    public float ledgeDisableDelay = 2f;
    private bool isDead = false;

    private void Update()
    {
        if (healthBar.BarProgress <= 0 && !isDead)
        {
            isDead = true;
            HandleDeath();
        }
    }

    private void HandleDeath()
    {
        if (deathSound != null)
        {
            deathSound.loop = true;
            deathSound.Play();
        }

        // Disable the Music GameObject to stop the background music
        if (musicGameObject != null)
        {
            musicGameObject.SetActive(false);
        }

        if (healthWarningScript != null) healthWarningScript.enabled = false;

        DisableLightControls();

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
        // Logic to disable B and F keys
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead && other.CompareTag("Ledge"))
        {
            StartCoroutine(DisableLedgeAfterDelay(other.gameObject));
        }
    }

    private IEnumerator DisableLedgeAfterDelay(GameObject ledge)
    {
        yield return new WaitForSeconds(ledgeDisableDelay);
        ledge.SetActive(false);
    }
}
