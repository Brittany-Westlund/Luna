using UnityEngine;
using System.Collections;

public class NeverDeactivate : MonoBehaviour
{
    private bool reactivating = false;

    private void OnDisable()
    {
        if (reactivating) return;
        StartCoroutine(ReenableNextFrame());
    }

    private IEnumerator ReenableNextFrame()
    {
        reactivating = true;
        yield return null; // wait one frame
        if (!gameObject.activeSelf)
        {
            Debug.LogWarning($"{name} was deactivated — restoring after Dialogue System call.");
            gameObject.SetActive(true);
        }
        reactivating = false;
    }
}
