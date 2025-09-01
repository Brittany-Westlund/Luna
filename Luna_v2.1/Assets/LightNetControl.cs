using UnityEngine;
using System.Linq; // Enables the use of Contains

public class LightNetController : MonoBehaviour
{
    public GameObject net;           // Reference to the Net object
    public GameObject lightMote;     // Reference to the LightMote sprite object under Net
    public string[] lanternTags = { "Lantern", "OutdoorLantern", "IndoorLantern" };

    private bool isLit = false;      // Tracks whether the net is "lit"

    private void Start()
    {
        // Initial state: Net on, LightMote off
        if (net != null) net.GetComponent<SpriteRenderer>().enabled = true;
        if (lightMote != null) lightMote.GetComponent<SpriteRenderer>().enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (lanternTags.Contains(other.tag) && isLit)
        {
            // Check if the lantern is already lit
            SpriteRenderer lanternSprite = other.GetComponent<SpriteRenderer>();
            if (lanternSprite != null && !lanternSprite.enabled)
            {
                // If the lantern is not already lit, activate it
                lanternSprite.enabled = true;

                // Reset Net and LightMote sprites
                if (lightMote != null) lightMote.GetComponent<SpriteRenderer>().enabled = false;
                isLit = false; // Net is no longer "lit"
            }
            else
            {
                // Lantern is already lit, cannot activate it again
            }
        }
        else if (other.CompareTag("LightMote") && !isLit)
        {
            // Collect the LightMote and activate the "lit" state
            Destroy(other.gameObject);  // Destroy the LightMote object
            if (lightMote != null) lightMote.GetComponent<SpriteRenderer>().enabled = true;
            isLit = true; // Mark the net as "lit"
        }
        else if (other.CompareTag("LightMote") && isLit)
        {
            // LightMote collection denied due to already having an active "lit" state
        }
    }
}
