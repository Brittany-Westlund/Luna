using UnityEngine;
using System.Collections;

public class Aid : MonoBehaviour
{
    public string aidTag = "Friendlies"; // Tag for objects to aid
    public float aidRadius = 5f; // Range of aid action
    public float sporeLaunchDistance = 2.0f; // Distance the spore moves forward
    public float sporeMoveSpeed = 5.0f; // Speed of spore movement
    public GameObject attachedSpore; // Reference to the spore, if attached

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) // Press 'A' to aid or launch spore
        {
            if (attachedSpore != null)
            {
                StartCoroutine(LaunchSpore());
            }
            else
            {
                AttemptAid();
            }
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

    private IEnumerator LaunchSpore()
    {
        if (attachedSpore == null) yield break;

        // Detach the spore from Luna
        attachedSpore.transform.SetParent(null);

        // Get the direction Luna is facing
        Vector3 moveDirection = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Vector3 targetPosition = attachedSpore.transform.position + moveDirection * sporeLaunchDistance;

        // Move the spore forward
        while (Vector3.Distance(attachedSpore.transform.position, targetPosition) > 0.01f)
        {
            attachedSpore.transform.position = Vector3.MoveTowards(
                attachedSpore.transform.position,
                targetPosition,
                sporeMoveSpeed * Time.deltaTime
            );
            yield return null;
        }

        // Destroy the spore after moving
        Destroy(attachedSpore);
        attachedSpore = null;

        Debug.Log("Spore launched and destroyed.");
    }

    public void AttachSpore(GameObject spore)
    {
        attachedSpore = spore;
    }

    public void DetachSpore()
    {
        attachedSpore = null;
    }
}
