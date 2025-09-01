using UnityEngine;
using System.Collections;

public class Aid : MonoBehaviour
{
    public float aidDistance = 3f; // Distance the spore moves when aiding
    public float moveSpeed = 5f; // Speed of the spore's movement

    private PresentSpore presentSpore; // Reference to the PresentSpore script
    private Transform lunaTransform; // Reference to Luna's transform

    void Start()
    {
        presentSpore = GetComponent<PresentSpore>();
        lunaTransform = transform; // Assuming this script is attached to Luna
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && presentSpore.HasSporeAttached && !presentSpore.IsSliding)
        {
            GameObject attachedSpore = presentSpore.GetAttachedSpore();
            if (attachedSpore != null)
            {
                Vector3 direction = lunaTransform.localScale.x > 0 ? Vector3.right : Vector3.left; // Determine Luna's facing direction
                StartCoroutine(SlideSporeForwardAndDestroy(attachedSpore, direction));
                presentSpore.ResetSporeState();
            }
        }
    }

    private IEnumerator SlideSporeForwardAndDestroy(GameObject spore, Vector3 direction)
    {
        Vector3 targetPosition = spore.transform.position + direction * aidDistance;

        while (Vector3.Distance(spore.transform.position, targetPosition) > 0.01f)
        {
            spore.transform.position = Vector3.MoveTowards(
                spore.transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        Destroy(spore);
        Debug.Log("Spore moved forward and was destroyed.");
    }
}
