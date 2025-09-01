using UnityEngine;
using System.Collections;
using MoreMountains.Tools;


public class Aid : MonoBehaviour
{
    public string aidTag = "Friendlies"; // Tag for objects to aid
    public float aidRadius = 5f; // Range of aid action
    public float sporeLaunchDistance = 2.0f; // Distance the spore moves forward
    public float sporeMoveSpeed = 5.0f; // Speed of spore movement

    private PresentSpore presentSpore; // Reference to PresentSpore script
    private GameObject attachedSpore; // Reference to the currently attached spore

    void Start()
    {
        // Find the PresentSpore script on the same GameObject
        presentSpore = GetComponent<PresentSpore>();
        if (presentSpore == null)
        {
            Debug.LogError("PresentSpore script not found on this GameObject!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) // Press 'A' to attempt an aid action
        {
            if (presentSpore != null && presentSpore.HasSporeAttached())
            {
                LaunchSpore();
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

    void LaunchSpore()
    {
        attachedSpore = presentSpore.GetAttachedSpore(); // Get the spore from PresentSpore
        if (attachedSpore == null)
        {
            Debug.LogError("No spore attached to launch!");
            return;
        }

        // Detach the spore and launch it forward
        StartCoroutine(LaunchSporeCoroutine());
    }

    IEnumerator LaunchSporeCoroutine()
    {
        if (attachedSpore == null) yield break;

        presentSpore.DetachSpore(); // Detach the spore from Luna

        Vector3 moveDirection = transform.localScale.x > 0 ? Vector3.right : Vector3.left;
        Vector3 targetPosition = attachedSpore.transform.position + moveDirection * sporeLaunchDistance;

        while (Vector3.Distance(attachedSpore.transform.position, targetPosition) > 0.01f)
        {
            attachedSpore.transform.position = Vector3.MoveTowards(
                attachedSpore.transform.position,
                targetPosition,
                sporeMoveSpeed * Time.deltaTime
            );
            yield return null;
        }

        Destroy(attachedSpore);
        attachedSpore = null;
        Debug.Log("Spore launched and destroyed.");
    }
}
