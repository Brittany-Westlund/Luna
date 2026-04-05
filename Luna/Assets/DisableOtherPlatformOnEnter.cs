using UnityEngine;

public class DisableOtherPlatformWithDelay : MonoBehaviour
{
    [Header("Assign the platform to disable")]
    public Collider2D platformToDisable;

    [Header("Player Tag")]
    public string playerTag = "Player";

    [Header("Time Required Standing")]
    public float requiredStandTime = 0.5f;

    private float standTimer = 0f;
    private bool playerOnPlatform = false;
    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag(playerTag))
        {
            playerOnPlatform = true;
            standTimer = 0f;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerOnPlatform = false;
            standTimer = 0f;
        }
    }

    private void Update()
    {
        if (hasTriggered || !playerOnPlatform)
            return;

        standTimer += Time.deltaTime;

        if (standTimer >= requiredStandTime)
        {
            if (platformToDisable != null)
            {
                platformToDisable.enabled = false;
            }

            hasTriggered = true;
        }
    }
}