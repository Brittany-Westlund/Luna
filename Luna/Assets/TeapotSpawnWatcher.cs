using UnityEngine;
using PixelCrushers.DialogueSystem;

public class TeapotSpawnWatcher : MonoBehaviour
{
    public string teapotObjectName = "Teapot(Clone)";  // Match your actual spawned object name
    private bool hasTriggered = false;

    void Update()
    {
        if (hasTriggered) return;

        GameObject teapot = GameObject.Find(teapotObjectName);
        if (teapot != null)
        {
            DialogueLua.SetVariable("FirstTeapotSpawned", true);
            Debug.Log("✅ FirstTeapotSpawned = true (via DialogueLua)");
            hasTriggered = true;
            Destroy(this); // Optional: remove this script after it triggers once
        }
    }
}
