using UnityEngine;

public class FaceLunaOnCollision : MonoBehaviour
{
    public Transform luna;  // Reference to Luna's transform

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object colliding is Luna
        if (collision.transform == luna)
        {
            // Get the direction to Luna (positive or negative on x-axis)
            Vector3 directionToLuna = luna.position - transform.position;

            // If Luna is to the right of NPC, face right; otherwise, face left
            if (directionToLuna.x > 0)
            {
                // Face right (ensure the localScale is positive on the x-axis)
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if (directionToLuna.x < 0)
            {
                // Face left (ensure the localScale is negative on the x-axis)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }
}
