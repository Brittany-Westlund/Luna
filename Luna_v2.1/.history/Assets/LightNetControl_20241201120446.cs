using UnityEngine;
using System.Linq;  // Enables the use of Contains

public class LightNetController : MonoBehaviour
{
    public GameObject net;           // Reference to the Net child object
    public GameObject litNet;        // Reference to the LitNet child object
    public GameObject lightMote;     // Reference to the LightMote sprite object under LightNet
    public string[] lanternTags = { "Lantern", "OutdoorLantern", "IndoorLantern" };

    private void Start()
    {
        // Initial state: Net on, LitNet and LightMote off
        if (net != null) net.GetComponent<SpriteRenderer>().enabled = true;
        if (litNet != null) litNet.GetComponent<SpriteRenderer>().enabled = false;
        if (lightMote != null) lightMote.GetComponent<SpriteRenderer>().enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the tag of 'other' matches any tag in the lanternTags array using Contains
        if (lanternTags.Contains(other.tag) && litNet.GetComponent<SpriteRenderer>().enabled)
        {
            // Check if the lantern is already lit
            SpriteRenderer lanternSprite = other.GetComponent<SpriteRenderer>();
            if (lanternSprite != null && !lanternSprite.enabled)
            {
                // If the lantern is not already lit, activate it
                lanternSprite.enabled = true;

                // Reset LitNet and LightMote sprites
                if (litNet != null) litNet.GetComponent<SpriteRenderer>().enabled = false;
                if (lightMote != null) lightMote.GetComponent<SpriteRenderer>().enabled = false;
            }
            else
            {
                // Lantern is already lit, cannot activate it again
            }
        }
        else if (other.CompareTag("LightMote") && !litNet.GetComponent<SpriteRenderer>().enabled)
        {
            // Collect the LightMote and activate the LitNet and LightMote sprites
            Destroy(other.gameObject);  // Destroy the LightMote object
            if (litNet != null) litNet.GetComponent<SpriteRenderer>().enabled = true;
            if (lightMote != null) lightMote.GetComponent<SpriteRenderer>().enabled = true;
        }
        else if (other.CompareTag("LightMote") && litNet.GetComponent<SpriteRenderer>().enabled)
        {
            // LightMote collection denied due to already having an active LitNet
        }
    }
}
