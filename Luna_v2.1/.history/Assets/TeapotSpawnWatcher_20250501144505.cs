using UnityEngine;
using PixelCrushers.DialogueSystem;

public class TeapotSpawnWatcher : MonoBehaviour
{
    [Tooltip("Assign the teapot prefab or tag if you want to search dynamically.")]
    public string teapotObjectName = "Teapot(Clone)";  // Adjust this if needed

    private bool hasTriggered = false;

    void Update()
    {
        if (hasTriggered) return;

        GameObject teapot = GameObject.Find(teapotObjectName);
        if (teapot != null)
        {
            Lua.Run("FirstTeapotSpawned = true");
            Debug.Log("TeapotSpawnWatcher: FirstTeapotSpawned = true");
            hasTriggered = true;

            // Optional: destroy or disable this script after it runs once
            Destroy(this);
        }
    }
}
