using UnityEngine;

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
