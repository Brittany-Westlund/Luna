using UnityEngine;
using UnityEngine.SceneManagement;

public class WandStateManager : MonoBehaviour
{
    public static WandStateManager Instance;

    public bool hasBeenPickedUp = false;
    public bool isCurrentlyHeld = false; // <-- new flag

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!hasBeenPickedUp || !isCurrentlyHeld) return;

        var luna = GameObject.FindWithTag("Player")?.transform;
        var attractor = GetComponent<LunariaWandAttractor>();
        if (luna != null && attractor != null)
        {
            var fly = Object.FindObjectOfType<ButterflyFlyHandler>();
            bool flying = fly != null && fly._isFlying;
            Transform holdPoint = flying ? attractor.flightHoldPoint : attractor.groundHoldPoint;

            if (holdPoint != null)
            {
                attractor.SendMessage("SnapUnder", holdPoint);
            }
        }
    }
}
