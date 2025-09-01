using UnityEngine;

public class Aid : MonoBehaviour
{
    public string aidTag = "Friendlies"; // Tag for objects to aid
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
        // Find all objects with the specified tag
        GameObject[] friendlies = GameObject.FindGameObjectsWithTag(aidTag);
        bool aidProvided = false;

        foreach (GameObject friendly in friendlies)
        {
            float distance = Vector3.Distance(transform.position, friendly.transform.position);
            if (distance <= aidRadius)
            {
                PerformAid(friendly);
                aidProvided = true;
            }
        }

        if (!aidProvided)
        {
            Debug.Log("No friendlies in range to aid.");
        }
    }

    void PerformAid(GameObject friendly)
    {
        // Logic for aiding/healing the object
        Debug.Log($"Aiding {friendly.name}!");
        // Add any specific aid effects here
    }
}
