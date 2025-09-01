using UnityEngine;

public class TurnOffOnKeyPress : MonoBehaviour
{
    [Tooltip("Key to press for this action.")]
    public KeyCode keyToPress = KeyCode.E;
    [Tooltip("If true, disables this entire GameObject; otherwise just its SpriteRenderer.")]
    public bool disableWholeObject = true;

    [Header("Optional Wand Toggle")]
    [Tooltip("If assigned, pressing the key will also call ToggleWand() on this wand.")]
    public LunariaWandAttractor wandToToggle;

    bool playerInTrigger = false;

    void Update()
    {
        if (!playerInTrigger || !Input.GetKeyDown(keyToPress))
            return;

        // 1) turn off the icon (or object)
        if (disableWholeObject)
            gameObject.SetActive(false);
        else if (TryGetComponent<SpriteRenderer>(out var sr))
            sr.enabled = false;

        // 2) if you hooked up a wand here, toggle pickup/drop
        if (wandToToggle != null)
            wandToToggle.ToggleWand();
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


/* using UnityEngine;

public class TurnOffOnKeyPress : MonoBehaviour
{
    public KeyCode keyToPress = KeyCode.E;
    public bool disableWholeObject = true; // If false, only disables the SpriteRenderer

    private bool playerInTrigger = false;

    private void Update()
    {
        if (playerInTrigger && Input.GetKeyDown(keyToPress))
        {
            if (disableWholeObject)
            {
                gameObject.SetActive(false);
            }
            else
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.enabled = false;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInTrigger = false;
        }
    }
}
*/