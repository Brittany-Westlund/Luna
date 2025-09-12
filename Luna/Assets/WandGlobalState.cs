using UnityEngine;

public class WandGlobalState : MonoBehaviour
{
    public static WandGlobalState Instance;

    [Header("Global Wand State")]
    public bool hasWand = false;
    public bool wandLit = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
