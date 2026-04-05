using UnityEngine;
using System.Collections;

public class DisableAfterDelay : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float delay = 2f;

    [Header("Behavior")]
    [SerializeField] private bool disableSelf = true;
    [SerializeField] private GameObject[] additionalObjectsToDisable;

    private void OnEnable()
    {
        StartCoroutine(DisableRoutine());
    }

    private IEnumerator DisableRoutine()
    {
        yield return new WaitForSeconds(delay);

        // Disable any additional objects
        if (additionalObjectsToDisable != null)
        {
            for (int i = 0; i < additionalObjectsToDisable.Length; i++)
            {
                if (additionalObjectsToDisable[i] != null)
                {
                    additionalObjectsToDisable[i].SetActive(false);
                }
            }
        }

        // Disable this object
        if (disableSelf)
        {
            gameObject.SetActive(false);
        }
    }
}