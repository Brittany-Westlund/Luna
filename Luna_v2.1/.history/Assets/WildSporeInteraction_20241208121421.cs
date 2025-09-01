using UnityEngine;


public class WildSporeInteraction : MonoBehaviour
{
    public LayerMask sporeLayer; // Assign this in the Inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & sporeLayer) != 0) // Check if in SporeLayer
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

