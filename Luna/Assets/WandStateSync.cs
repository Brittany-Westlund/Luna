using UnityEngine;

public class WandStateSync : MonoBehaviour
{
    private LunariaWandAttractor attractor;

    void Awake()
    {
        attractor = GetComponent<LunariaWandAttractor>();
    }

    void Update()
    {
        if (attractor == null || WandGlobalState.Instance == null)
            return;

        // Mirror the current state into the global state
        WandGlobalState.Instance.wandLit = attractor.HasLight();
    }
}
