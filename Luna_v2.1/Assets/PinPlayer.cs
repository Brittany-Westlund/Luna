using UnityEngine;
using MoreMountains.CorgiEngine;

public class PinPlayer : MonoBehaviour
{
    public Transform pinPosition; // Assign the position where the player should be pinned
    private Character character;
    private bool isPinned = false;

    private void Start()
    {
        // Find the Character component (part of Corgi Engine) on the player
        character = FindObjectOfType<Character>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Pin the player at the specified position
            character.transform.position = pinPosition.position;
            character.Freeze(); // Disable movement
            isPinned = true;
        }
    }

    private void Update()
    {
        if (isPinned && (Input.GetAxis("Horizontal") != 0))
        {
            // Allow the player to move again
            character.UnFreeze();
            isPinned = false;
        }
    }
}
