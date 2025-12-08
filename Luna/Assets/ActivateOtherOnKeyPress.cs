using UnityEngine;

public class ActivateOtherOnKeyPress : MonoBehaviour
{
    public KeyCode keyToPress = KeyCode.E;

    [Header("Object To Activate")]
    public GameObject objectToActivate;

    [Header("How This Object Turns Off")]
    public bool disableWholeObject = true; // If false, only disables SpriteRenderer

    private bool playerInTrigger = false;

    private void Update()
    {
        if (playerInTrigger && Input.GetKeyDown(keyToPress))
        {
            // ✅ TURN ON TARGET FIRST
            if (objectToActivate != null)
            {
                objectToActivate.SetActive(true);
            }

            // ✅ THEN DISABLE THIS OBJECT
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

    private void OnTriggerStay2D(Collider2D other)
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
