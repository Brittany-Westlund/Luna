using UnityEngine;

public class TurnOnOnKeyPressFlexible : MonoBehaviour
{
    public KeyCode keyToPress = KeyCode.E;

    [Header("Enable Settings")]
    public bool enableWholeObject = false; // If true, enables targetObject entirely
    public bool enableSelfRenderer = true; // If true, enables this object's renderer

    [Tooltip("Optional object to enable entirely")]
    public GameObject targetObject;

    private bool playerInTrigger = false;

    void Update()
    {
        if (playerInTrigger && Input.GetKeyDown(keyToPress))
        {
            // Enable this object's renderer (SpriteRenderer or MeshRenderer)
            if (enableSelfRenderer)
            {
                Renderer r = GetComponent<Renderer>();
                if (r != null)
                    r.enabled = true;
                else
                    Debug.LogWarning($"{name}: No Renderer found to enable.");
            }

            // Enable another object entirely
            if (enableWholeObject && targetObject != null)
            {
                targetObject.SetActive(true);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInTrigger = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInTrigger = false;
    }
}
