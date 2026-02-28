using UnityEngine;

public class FirstBookPersists : MonoBehaviour
{
    private static bool firstBookExists = false;

    void Awake()
    {
        if (!firstBookExists)
        {
            // This is the first book.
            firstBookExists = true;
            return;
        }

        // Another book already exists → destroy this one.
        if (CompareTag("Book"))
        {
            Destroy(gameObject);
        }
    }
}