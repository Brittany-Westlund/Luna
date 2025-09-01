using UnityEngine;

public class WildSporeInteraction : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sprout"))
        {
            Debug.Log("Wild spore collided with a sprout!");

            AidingSprouts sproutScript = other.GetComponent<AidingSprouts>();
            if (sproutScript != null)
            {
                sproutScript.Grow(); // Trigger the growth function
                Destroy(gameObject); // Destroy wild spore after interaction
            }
        }
    }
}
