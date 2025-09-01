using UnityEngine;

public class FoxglovePollenPickup : MonoBehaviour
{
    public float duration = 30f; // Optional: duration of effect

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FairyPollenStatus pollenStatus = other.GetComponent<FairyPollenStatus>();
            if (pollenStatus != null)
            {
                pollenStatus.GiveFairypetalPollen(); // Enable pollen access
                Debug.Log("🧚‍♀️ Foxglove pollen acquired!");

                // Optional: clear after a duration
                pollenStatus.Invoke("ClearFairypetalPollen", duration);
            }

            Destroy(gameObject); // Remove the pickup from the world
        }
    }
}
