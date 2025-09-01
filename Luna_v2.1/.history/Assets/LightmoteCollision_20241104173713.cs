using UnityEngine;

public class LightMoteManager : MonoBehaviour
{
    public GameObject lightNet;         // Reference to the LightNet object
    public GameObject lightMotePrefab;  // Reference to the LightMote prefab

    // Method to attach a new LightMote to LightNet if it doesn't already have one
    public void AttachLightMote()
    {
        // Check if LightNet already has a LightMote
        if (lightNet.transform.Find("LightMote") != null)
        {
            Debug.Log("LightNet already has a LightMote. Cannot add another.");
            return; // Exit if LightNet already has a LightMote
        }

        // Instantiate and attach the LightMote to LightNet
        GameObject newLightMote = Instantiate(lightMotePrefab, lightNet.transform);
        newLightMote.name = "LightMote"; // Ensure the name is consistent
        Debug.Log("New LightMote attached to LightNet.");
    }
}
