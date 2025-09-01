using UnityEngine;
using System.Collections;

public class Aid : MonoBehaviour
{
    public string aidTag = "Friendlies"; // Tag for objects to aid
    public float sporeLaunchDistance = 2.0f; // Distance the spore moves forward
    public float sporeMoveSpeed = 5.0f; // Speed of spore movement

    private PresentSpore presentSpore; // Reference to PresentSpore script
    private GameObject attachedSpore; // Reference to the currently attached spore

    void Start()
    {
        presentSpore = GetComponent<PresentSpore>();
        if (presentSpore == null)
        {
            Debug.LogError("PresentSpore script not found on this GameObject!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A)) // Press 'A' to aid
        {
            if (presentSpore != null && presentSpore.HasSporeAttached)
            {
                LaunchSpore();
            }
        }
    }

    void LaunchSpore()
    {
        if (presentSpore.IsSliding)
        {
            Debug.LogWarning("Cannot launch spore while it is still sliding.");
            return;
        }

        attachedSpore = presentSpore.GetAttachedSpore();
        if (attachedSpore == null)
        {
            Debug.LogError("No spore attached to launch!");
            return;
        }

        StartCoroutine(LaunchSporeCoroutine());
    }

    IEnumerator LaunchSporeCoroutine()
    {
        presentSpore.DetachSporeWithSlide();

        yield return new WaitUntil(() => !presentSpore.IsSliding);

        // Determine the direction Luna is facing
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

        Debug.Log("Spore successfully launched forward and destroyed.");
    }
}
