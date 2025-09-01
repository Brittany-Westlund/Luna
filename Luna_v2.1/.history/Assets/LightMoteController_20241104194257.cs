using UnityEngine;

public class LightMoteManager : MonoBehaviour
{
    public GameObject lightNet;         // Reference to the LightNet object
    public GameObject lightMotePrefab;  // Reference to the LightMote prefab
    private GameObject currentLightMote; // Store the currently collected LightMote

    // Method to collect a new LightMote if one isn't already attached
    public void CollectLightMote()
    {
        // Check if LightNet already has a LightMote
        if (currentLightMote != null)
        {
            Debug.Log("LightNet already has a LightMote. Cannot collect another.");
            return; // Exit if there's already a LightMote
        }

        // Instantiate a new LightMote and attach it to LightNet
        currentLightMote = Instantiate(lightMotePrefab, lightNet.transform);
        currentLightMote.name = "LightMote"; // Consistent naming
        currentLightMote.tag = "LightMote";  // Tagging for identification
        currentLightMote.transform.localPosition = Vector3.zero; // Center it within LightNet
        Debug.Log("New LightMote collected and attached to LightNet.");
    }

    // Method to release the LightMote after use (e.g., after lighting a Lantern)
    public void ReleaseLightMote()
    {
        if (currentLightMote != null)
        {
            Destroy(currentLightMote);
            currentLightMote = null;
            Debug.Log("LightMote released from LightNet.");
        }
    }
}
