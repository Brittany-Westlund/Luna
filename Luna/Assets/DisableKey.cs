using UnityEngine;

public class DisableKey : MonoBehaviour
{
    [Tooltip("Which key to disable.")]
    public KeyCode keyToDisable = KeyCode.B;

    [Tooltip("Whether the key is currently disabled.")]
    public bool disableKey = true;

    void Update()
    {
        if (disableKey && Input.GetKeyDown(keyToDisable))
        {
            // Eat the keypress – do nothing.
            // (You can also log it if you want confirmation)
            // Debug.Log($"{keyToDisable} is disabled in this scene.");
        }
    }
}
