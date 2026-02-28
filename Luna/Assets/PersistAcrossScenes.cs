using UnityEngine;

public class PersistAcrossScenes : MonoBehaviour
{
    private static PersistAcrossScenes _instance;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject); // prevent duplicates when entering a new scene
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}