using UnityEngine;

public class Aid : MonoBehaviour
{
    public GameObject target; // The object to aid or heal
    public float aidRadius = 5f; // Range of aid action

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) // Press 'A' to aid
        {
            AttemptAid();
        }
    }

    void AttemptAid()
    {
        if (target != null && Vector3.Distance(transform.position, target.transform.position) <= aidRadius)
        {
            Debug.Log("Aid provided!");
            PerformAid();
        }
        else
        {
            Debug.Log("No target in range.");
        }
    }

    void PerformAid()
    {
        // Logic for aiding/healing target
        Debug.Log($"Aiding {target.name}!");
    }
}
