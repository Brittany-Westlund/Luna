using UnityEngine;

public class LightNetControl : MonoBehaviour
{
    public GameObject lightMotePrefab; // Reference to the LightMote prefab

    private void Update()
    {
        // Check if there is more than one LightMote as a child
        int lightMoteCount = CountLightMotes();
        if (lightMoteCount > 1)
        {
            Debug.LogWarning("Multiple LightMotes detected. Keeping only one.");
            RemoveExtraLightMotes();
        }
    }

    // Method to count the number of LightMote children
    private int CountLightMotes()
    {
        int count = 0;
        foreach (Transform child in transform)
        {
            if (child.name == "LightMote")
            {
                count++;
            }
        }
        return count;
    }

    // Method to destroy extra LightMotes and keep only one
    private void RemoveExtraLightMotes()
    {
        bool firstMoteFound = false;
        foreach (Transform child in transform)
        {
            if (child.name == "LightMote")
            {
                if (!firstMoteFound)
                {
                    firstMoteFound = true; // Keep the first LightMote found
                }
                else
                {
                    Destroy(child.gameObject); // Destroy any additional LightMotes
                }
            }
        }
    }
}
