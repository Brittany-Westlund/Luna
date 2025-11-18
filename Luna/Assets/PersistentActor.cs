using UnityEngine;
using System.Collections;

public class PersistentActor : MonoBehaviour
{
    private void OnEnable()
    {
        // Run the restore coroutine AFTER Unity finishes reactivating this object.
        StartCoroutine(RestoreNextFrame());
    }

    IEnumerator RestoreNextFrame()
    {
        // one frame delay so DialogueSystem cleanup finishes
        yield return null;

        // If something else disabled it again this very frame, re-enable.
        if (!gameObject.activeSelf)
        {
            Debug.Log($"{name} re-enabled after DialogueSystem cleanup.");
            gameObject.SetActive(true);
        }
    }

    // No logic needed in OnDisable anymore
}
