using UnityEngine;

public class TurnOnOnKeyPress : MonoBehaviour
{
    public KeyCode keyToPress = KeyCode.E;
    public bool enableWholeObject = true; // If false, only enables the SpriteRenderer
    public GameObject targetObject; // The object to enable (if using enableWholeObject)

    private bool playerInTrigger = false;

    private void Update()
    {
        if (playerInTrigger && Input.GetKeyDown(keyToPress))
        {
            if (enableWholeObject)
            {
                if (targetObject != null)
                {
                    targetObject.SetActive(true);
                }
            }
            else
            {
                SpriteRenderer sr = GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.enabled = true;
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
