using UnityEngine;

public class LightmoteCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object has the tag "Lantern"
        if (other.CompareTag("Lantern"))
        {
            Debug.Log("Lightmote collided with Lantern!");
        }
    }
}
