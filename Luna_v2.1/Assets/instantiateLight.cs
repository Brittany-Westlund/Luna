using UnityEngine;

public class InstantiateLight : MonoBehaviour
{
    // Make sure this line is exactly as shown below, with no extra characters or syntax errors
    public GameObject lightMotePrefab; // This field should appear in the Inspector

    public void SpawnLightMote()
    {
        // Instantiate the light mote prefab
        if (lightMotePrefab != null)
        {
            GameObject lightMote = Instantiate(lightMotePrefab, transform.position, Quaternion.identity);
            Destroy(lightMote, 1f); // Destroy after 1 second
        }
        else
        {
            Debug.LogError("Light Mote prefab not assigned.");
        }
    }
}