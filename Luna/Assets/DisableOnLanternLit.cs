using UnityEngine;

public class DisableOnLanternLit : MonoBehaviour
{
    public SpriteRenderer litLantern;  // Reference to the LitLantern's SpriteRenderer
    public GameObject targetObject;    // The GameObject to disable

    void Update()
    {
        // Check if the LitLantern is enabled and the target object is still active
        if (litLantern.enabled && targetObject.activeSelf)
        {
            targetObject.SetActive(false);  // Disable the target object indefinitely
            Debug.Log(targetObject.name + " has been disabled because LitLantern is enabled.");
        }
    }
}
