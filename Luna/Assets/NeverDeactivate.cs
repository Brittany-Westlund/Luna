using UnityEngine;
using System.Collections;

public class NeverDeactivate : MonoBehaviour
{
    private bool reactivating = false;

    private void OnEnable()
    {
        // Safe, because object is active here
        StartCoroutine(ReenableNextFrame());
    }

    private IEnumerator ReenableNextFrame()
    {
        if (reactivating) yield break;
        reactivating = true;

        yield return null; // wait one frame

        // If something else (DialogueSystem) disabled it again
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning($"{name} was deactivated — restoring after Dialogue System call.");
            gameObject.SetActive(true);
        }

        reactivating = false;
    }

    // OnDisable no longer needed
}
