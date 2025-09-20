using UnityEngine;

public class KillFadeOnDestroy : MonoBehaviour
{
    void OnDestroy()
    {
        var healthMgr = FindObjectOfType<LunaHealthManager>();
        if (healthMgr != null)
        {
            Debug.Log("[KillFadeOnDestroy] Luna renderer destroyed, stopping fade coroutines.");
            healthMgr.StopAllCoroutines();
        }
    }
}
