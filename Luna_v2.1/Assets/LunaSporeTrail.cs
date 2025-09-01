using UnityEngine;
using System.Collections;

public class SporeDropper : MonoBehaviour
{
    public GameObject sporePrefab;  // Assign the spore prefab in the Inspector
    public float spawnDelay = 5f;   // Delay before a spore appears
    private Vector3 lastGroundedPosition; // Stores last valid ground position
    private bool isGrounded = false; // Tracks if Luna is on the ground

    private void Start()
    {
        lastGroundedPosition = transform.position; // Default to Luna's start position
        StartCoroutine(SpawnSporesOverTime()); // Start the spore dropping cycle
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            lastGroundedPosition = collision.ClosestPoint(transform.position); // Store exact ground point
            
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        
        }
    }

    private IEnumerator SpawnSporesOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnDelay);

            if (isGrounded)
            {
                DropSpore();
            }
        }
    }

    private void DropSpore()
    {
        if (sporePrefab == null)
        {
            Debug.LogWarning("⚠️ No spore prefab assigned!");
            return;
        }

        Instantiate(sporePrefab, lastGroundedPosition, Quaternion.identity);
        Debug.Log($"🍄 Spore spawned at {lastGroundedPosition}");
    }
}
