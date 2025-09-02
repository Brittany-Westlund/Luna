using UnityEngine;

public class ButterflyMoveToLuna : MonoBehaviour
{
    public Transform luna; // Assign Luna's transform in the inspector
    public float moveSpeed = 3f; // Speed at which the butterfly moves
    public float stopDistance = 0.5f; // Distance from Luna to stop at

    private bool shouldMove = false;

    void Update()
    {
        if (shouldMove && luna != null)
        {
            // Move the butterfly towards Luna
            float step = moveSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, luna.position, step);

            // Stop moving when close enough
            if (Vector3.Distance(transform.position, luna.position) <= stopDistance)
            {
                shouldMove = false;
            }
        }
    }

    public void MoveToLuna()
    {
        shouldMove = true;
    }
}
