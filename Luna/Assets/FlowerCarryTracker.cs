using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SproutAndLightManager))]
public class FlowerCarryTracker : MonoBehaviour
{
    private SproutAndLightManager sprout;
    private bool registeredAsHeld = false;

    void Awake()
    {
        sprout = GetComponent<SproutAndLightManager>();
    }

    void Update()
    {
        if (sprout == null) return;

        bool isHeld = sprout.isHeld;

        // Just picked up
        if (isHeld && !registeredAsHeld)
        {
            if (FlowerGlobalState.Instance != null)
            {
                FlowerGlobalState.Instance.RegisterHeldFlower(gameObject);
                registeredAsHeld = true;
            }
        }
        // Just dropped / planted
        else if (!isHeld && registeredAsHeld)
        {
            if (FlowerGlobalState.Instance != null)
            {
                FlowerGlobalState.Instance.ClearIfThis(gameObject);
            }

            // Move back into the active scene (it was in DontDestroyOnLoad)
            var active = SceneManager.GetActiveScene();
            SceneManager.MoveGameObjectToScene(gameObject, active);

            registeredAsHeld = false;
        }
    }
}
