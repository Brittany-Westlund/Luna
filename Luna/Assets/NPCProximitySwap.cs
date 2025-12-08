using UnityEngine;

public class NPCProximitySwap : MonoBehaviour
{
    public GameObject StartingObjectState;   // e.g. LumiaWorking
    public GameObject TriggeredObjectState;  // e.g. LumiaIdle

    private void Start()
    {
        if (TriggeredObjectState != null)
            TriggeredObjectState.SetActive(false);

        if (StartingObjectState != null)
            StartingObjectState.SetActive(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (StartingObjectState != null)
                StartingObjectState.SetActive(false);

            if (TriggeredObjectState != null)
                TriggeredObjectState.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (TriggeredObjectState != null)
                TriggeredObjectState.SetActive(false);

            if (StartingObjectState != null)
                StartingObjectState.SetActive(true);
        }
    }
}
