using UnityEngine;

public class ActivateOtherOnKeyPress : MonoBehaviour
{
    public KeyCode keyToPress = KeyCode.E;

    [Header("Object To Activate (Optional)")]
    public GameObject objectToActivate;

    [Header("Component To Enable (Optional)")]
    public Behaviour componentToEnable;
    // Drag any component here (e.g., DialogueTrigger script). Must be a Behaviour.

    public enum DisableMode
    {
        DisableWholeObject,
        DisableSpriteRenderer,
        DoNothing
    }

    [Header("How This Object Turns Off")]
    public DisableMode disableMode = DisableMode.DisableWholeObject;

    private bool playerInTrigger = false;
    private bool hasActivated = false; // ONE-SHOT LATCH

    private void Update()
    {
        if (hasActivated) return;

        if (playerInTrigger && Input.GetKeyDown(keyToPress))
        {
            hasActivated = true;

            // 1) Activate target GameObject (if assigned)
            if (objectToActivate != null)
            {
                objectToActivate.SetActive(true);
            }

            // 2) Enable target Component (if assigned)
            if (componentToEnable != null)
            {
                componentToEnable.enabled = true;
            }

            // 3) Disable behavior for THIS object
            switch (disableMode)
            {
                case DisableMode.DisableWholeObject:
                    gameObject.SetActive(false);
                    break;

                case DisableMode.DisableSpriteRenderer:
                    SpriteRenderer sr = GetComponent<SpriteRenderer>();
                    if (sr != null)
                        sr.enabled = false;
                    break;

                case DisableMode.DoNothing:
                    // Intentionally do nothing
                    break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInTrigger = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInTrigger = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInTrigger = false;
    }
}