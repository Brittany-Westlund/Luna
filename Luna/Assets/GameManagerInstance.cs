using UnityEngine;

public class GameManagerInstance : MonoBehaviour
{
    public static GameManagerInstance Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("❌ Destroying extra GameManager: " + gameObject.name);
            Destroy(gameObject); // Kill the extra one
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("✅ GameManager is now persistent.");
    }
}
