using UnityEngine;

public class PresentSpore : MonoBehaviour
{
    public GameObject sporePrefab; // The prefab to instantiate as the spore
    public Transform attachPoint; // The point where the spore will attach to Luna
    private GameObject activeSpore; // Reference to the currently instantiated spore

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S)) // Press 'S' to create the spore
        {
            if (activeSpore == null) // Only instantiate if no spore exists
            {
                CreateSpore();
            }
            else
            {
                Debug.Log("A spore is already present!");
            }
        }

        if (Input.GetKeyDown(KeyCode.D)) // Optional: Press 'D' to detach/remove the spore
        {
            DetachSpore();
        }
    }

    void CreateSpore()
    {
        if (sporePrefab == null || attachPoint == null)
        {
            Debug.LogError("Spore prefab or attach point is not assigned!");
            return;
        }

        // Instantiate the spore and parent it to the attach point
        activeSpore = Instantiate(sporePrefab, attachPoint.position, attachPoint.rotation);
        activeSpore.transform.SetParent(attachPoint);

        Debug.Log($"Spore {activeSpore.name} created and attached to {attachPoint.name}");
    }

    void DetachSpore()
    {
        if (activeSpore != null)
        {
            activeSpore.transform.SetParent(null); // Detach the spore
            Destroy(activeSpore); // Destroy the spore
            activeSpore = null;

            Debug.Log("Spore detached and removed.");
        }
    }
}
