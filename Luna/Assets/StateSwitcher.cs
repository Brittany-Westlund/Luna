using UnityEngine;

public class StateSwitcher : MonoBehaviour
{
    public GameObject initialState; // The initial state of the object (e.g., DryGround)
    public GameObject newStatePrefab; // The prefab for the new state (e.g., WetGround)
    public KeyCode switchKey = KeyCode.X; // Key to switch states
    public GameObject collisionTarget; // The GameObject DryGround must be colliding with to switch states

    private void Update()
    {
        if (Input.GetKeyDown(switchKey) && initialState.activeInHierarchy) // Check if the initial state is active
        {
            SwitchState();
        }
    }

    void SwitchState()
    {
        // Check if DryGround is colliding with the specified target
        Collider2D[] colliders = Physics2D.OverlapCircleAll(initialState.transform.position, 0.1f);
        foreach (var collider in colliders)
        {
            if (collider.gameObject == collisionTarget)
            {
                // Instantiate the new state at the same position
                GameObject newState = Instantiate(newStatePrefab, initialState.transform.position, Quaternion.identity);

                // Optionally, destroy the initial state
                Destroy(initialState);

                // Optionally, disable or destroy the initial state
                initialState.SetActive(false);

                break; // Exit the loop after finding the collision target
            }
        }
    }
}
