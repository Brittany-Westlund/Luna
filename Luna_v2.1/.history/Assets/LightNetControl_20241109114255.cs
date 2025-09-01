using UnityEngine;

public class LightNetController : MonoBehaviour
{
    public GameObject net;           // Reference to the Net child object
    public GameObject litNet;        // Reference to the LitNet child object
    public GameObject lightMote;     // Reference to the LightMote sprite object under LightNet

    private void Start()
    {
        // Initial state: Net on, LitNet and LightMote off
        if (net != null) net.GetComponent<SpriteRenderer>().enabled = true;
        if (litNet != null) litNet.GetComponent<SpriteRenderer>().enabled = false;
        if (lightMote != null) lightMote.GetComponent<SpriteRenderer>().enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the net is colliding with a LightMote and if the LitNet is inactive
        if (other.CompareTag("LightMote") && litNet.GetComponent<SpriteRenderer>().enabled == false)
        {
            // Collect the LightMote and activate the LitNet and LightMote sprites
            Destroy(other.gameObject);  // Destroy the LightMote object
            if (litNet != null) litNet.GetComponent<SpriteRenderer>().enabled = true;
            if (lightMote != null) lightMote.GetComponent<SpriteRenderer>().enabled = true;
            Debug.Log("LightMote collected. LitNet and LightMote sprites activated.");
        }
        else if (other.CompareTag("LightMote") && litNet.GetComponent<SpriteRenderer>().enabled == true)
        {
            // LightMote collection denied due to already having an active LitNet
            Debug.Log("Cannot collect another LightMote. Use the current one to light a lantern first.");
        }

        // Check if LitNet is active and colliding with a Lantern
        if (other.CompareTag("Lantern") && litNet.GetComponent<SpriteRenderer>().enabled)
        {
            // Turn on the Lantern's sprite renderer
            SpriteRenderer lanternSprite = other.GetComponent<SpriteRenderer>();
            if (lanternSprite != null)
            {
                lanternSprite.enabled = true;
                Debug.Log("Lantern activated.");
            }

            // Reset LitNet and LightMote sprites
            if (litNet != null) litNet.GetComponent<SpriteRenderer>().enabled = false;
            if (lightMote != null) lightMote.GetComponent<SpriteRenderer>().enabled = false;
            Debug.Log("LitNet and LightMote sprites turned off.");
        }
    }
}
