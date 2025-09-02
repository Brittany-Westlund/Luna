using UnityEngine;

public class LadderClimb : MonoBehaviour
{
    private bool isClimbing = false;
    private Rigidbody2D rb;
    private Transform player; // Reference to the player (Luna)

    void Update()
    {
        if (isClimbing && player != null)
        {
            // Allow player to move vertically while climbing
            float verticalInput = Input.GetAxis("Vertical");
            rb.velocity = new Vector2(0, verticalInput * rb.velocity.y);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) // Assuming the player is tagged as "Player"
        {
            isClimbing = true;
            player = collision.transform;
            rb = player.GetComponent<Rigidbody2D>();
            rb.gravityScale = 0; // Disable gravity while climbing
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isClimbing = false;
            rb.gravityScale = 1; // Re-enable gravity after climbing
            player = null;
        }
    }
}
