using UnityEngine;

public class BookPersistWhenStored : MonoBehaviour
{
    [Header("Auto")]
    public BookControllerSimple book;

    [Header("Debug")]
    public bool debugLogs = false;

    private bool didPersist = false;

    void Awake()
    {
        if (book == null) book = GetComponent<BookControllerSimple>();
    }

    void Update()
    {
        if (didPersist) return;
        if (book == null) return;

        // Only become persistent AFTER Luna stores it (book is now "with her")
        if (book.IsStored)
        {
            didPersist = true;
            DontDestroyOnLoad(gameObject);

            if (debugLogs) Debug.Log("📘 BookPersistWhenStored: now persistent (stored on Luna).");
        }
    }
}