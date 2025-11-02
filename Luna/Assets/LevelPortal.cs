using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPortal : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad;       // set in Inspector
    public string portalID;          // unique name like "MeadowToMire"

    [Header("Portal Settings")]
    public bool requireInteract = true;   // ✅ toggle in Inspector
    public KeyCode interactKey = KeyCode.E;
    public float autoDelay = 0f;          // delay before auto load (optional)

    [Header("UI Icon")]
    public GameObject interactIcon;

    private bool playerInRange = false;
    private bool hasUsed = false;

    void Start()
    {
        if (interactIcon != null)
            interactIcon.SetActive(false);

        var gm = GameObject.Find("GameManager");
        if (gm != null)
            DontDestroyOnLoad(gm);
    }

    void Update()
    {
        if (hasUsed) return; // prevent double-triggers

        if (requireInteract && playerInRange && Input.GetKeyDown(interactKey))
        {
            UsePortal();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasUsed || !other.CompareTag("Player")) return;

        playerInRange = true;

        if (requireInteract)
        {
            if (interactIcon != null)
                interactIcon.SetActive(true);
        }
        else
        {
            if (autoDelay > 0)
                Invoke(nameof(UsePortal), autoDelay);
            else
                UsePortal();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;
        if (interactIcon != null)
            interactIcon.SetActive(false);
    }

    private void UsePortal()
    {
        if (hasUsed) return;
        hasUsed = true;

        if (interactIcon != null)
            interactIcon.SetActive(false);

        PortalState.lastUsedPortal = portalID;
        Debug.Log($"[PORTAL] Using '{portalID}' → load '{sceneToLoad}'");

        SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
    }
}
