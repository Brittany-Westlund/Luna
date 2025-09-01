using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    public Transform teleporterEntryPoint; // Set this in the Unity Inspector
    public Transform teleportTarget; // Set this in the Unity Inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Move the player to the exact entry point
            other.transform.position = teleporterEntryPoint.position;

            // Optionally, use a coroutine if you need a delay before teleporting
            // StartCoroutine(TeleportPlayerAfterDelay(other.transform));
        }
    }

    // Uncomment and use this method if a delay is needed
    // private IEnumerator TeleportPlayerAfterDelay(Transform playerTransform)
    // {
    //     yield return new WaitForSeconds(0.1f); // short delay
    //     playerTransform.position = teleportTarget.position;
    // }
}
