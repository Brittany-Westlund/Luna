using UnityEngine;

public class WandForever : MonoBehaviour
{
    public GameObject wandChild;

    void Awake()
    {
        if (wandChild == null)
        {
            var attractor = GetComponentInChildren<LunariaWandAttractor>(true);
            if (attractor != null)
                wandChild = attractor.gameObject;
        }

        if (wandChild == null) return;

        if (WandGlobalState.Instance != null && WandGlobalState.Instance.hasWand)
        {
            wandChild.SetActive(true);

            var attractor = wandChild.GetComponent<LunariaWandAttractor>();
            if (attractor != null)
            {
                if (WandGlobalState.Instance.wandLit)
                {
                    attractor.ForceLit(); // wand lit
                }
                else
                {
                    attractor.ResetWandVisualsGlobal(); // 👈 renamed method
                }
            }
        }
        else
        {
            wandChild.SetActive(false);
        }
    }

    public void UnlockWand()
    {
        if (wandChild != null)
            wandChild.SetActive(true);

        if (WandGlobalState.Instance != null)
        {
            WandGlobalState.Instance.hasWand = true;
            WandGlobalState.Instance.wandLit = false; // start unlit
        }

        Debug.Log("✨ Luna now permanently has the wand.");
    }
}
