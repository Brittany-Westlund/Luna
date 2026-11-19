using UnityEngine;

public class SimpleBookToggle : MonoBehaviour
{
    [Header("UI")]
    public GameObject bookUIRoot;
    public CanvasGroup bookCanvasGroup;

    [Header("Page Controller")]
    public BookControllerSimple bookController;

    [Header("Movement Lock")]
    public MonoBehaviour lunaMovementScript;
    public string movementLockBoolName = "MovementForbidden";

    [Header("Input")]
    public KeyCode toggleKey = KeyCode.V;
    public bool allowArrowKeys = true;

    [Header("Debug")]
    public bool debugLogs = true;

    private bool isOpen = false;

    void Awake()
    {
        if (bookUIRoot != null && bookCanvasGroup == null)
            bookCanvasGroup = bookUIRoot.GetComponent<CanvasGroup>();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (debugLogs) Debug.Log("SimpleBookToggle: V pressed.");

            if (isOpen)
                CloseBook();
            else
                OpenBook();
        }

        if (!isOpen) return;

        if (bookController != null)
        {
            if (Input.GetKeyDown(KeyCode.D) || (allowArrowKeys && Input.GetKeyDown(KeyCode.RightArrow)))
                bookController.NextPage();

            if (Input.GetKeyDown(KeyCode.A) || (allowArrowKeys && Input.GetKeyDown(KeyCode.LeftArrow)))
                bookController.PreviousPage();
        }
    }

    public void OpenBook()
    {
        isOpen = true;

        if (bookController != null)
            bookController.IsExternallyOpen = true;

        if (bookUIRoot != null)
            bookUIRoot.SetActive(true);

        if (bookCanvasGroup != null)
        {
            bookCanvasGroup.alpha = 1f;
            bookCanvasGroup.interactable = true;
            bookCanvasGroup.blocksRaycasts = true;
        }

        SetMovementLocked(true);

        if (debugLogs) Debug.Log("SimpleBookToggle: Book opened.");
    }

    public void CloseBook()
    {
        isOpen = false;

        if (bookController != null)
            bookController.IsExternallyOpen = false;

        if (bookCanvasGroup != null)
        {
            bookCanvasGroup.alpha = 0f;
            bookCanvasGroup.interactable = false;
            bookCanvasGroup.blocksRaycasts = false;
        }

        if (bookUIRoot != null)
            bookUIRoot.SetActive(false);

        SetMovementLocked(false);

        if (debugLogs) Debug.Log("SimpleBookToggle: Book closed.");
    }

    void SetMovementLocked(bool locked)
    {
        if (lunaMovementScript == null) return;

        var type = lunaMovementScript.GetType();

        var field = type.GetField(movementLockBoolName);
        if (field != null && field.FieldType == typeof(bool))
        {
            field.SetValue(lunaMovementScript, locked);
            return;
        }

        var prop = type.GetProperty(movementLockBoolName);
        if (prop != null && prop.PropertyType == typeof(bool) && prop.CanWrite)
            prop.SetValue(lunaMovementScript, locked, null);
    }
}
