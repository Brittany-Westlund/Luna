using UnityEngine;

public class PersistentLuna : MonoBehaviour
{
    private void Awake()
    {
        // Make sure only one Luna exists
        var existing = FindObjectsOfType<PersistentLuna>();
        if (existing.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
}
