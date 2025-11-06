using UnityEngine;
using System.Collections;

public class PersistentActor : MonoBehaviour
{
    void OnDisable()
    {
        StartCoroutine(RestoreNextFrame());
    }

    IEnumerator RestoreNextFrame()
    {
        yield return null; // wait until DialogueSystem finishes cleanup
        if (!gameObject.activeSelf)
        {
            Debug.Log($"{name} re-enabled after DialogueSystem cleanup.");
            gameObject.SetActive(true);
        }
    }
}
