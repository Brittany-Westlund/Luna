using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPortal : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad; // set in Inspector
    public string portalID;    // unique name like "MeadowToMire"

    [Header("UI Icon")]
    public GameObject interactIcon;

    private bool playerInRange = false;

    void Start()
    {
        if (interactIcon != null) 
            interactIcon.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // Save which portal was used
            PortalState.lastUsedPortal = portalID;

            SceneManager.LoadScene(sceneToLoad);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactIcon != null) 
                interactIcon.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactIcon != null) 
                interactIcon.SetActive(false);
        }
    }
}