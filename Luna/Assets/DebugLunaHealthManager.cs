using UnityEngine;

public class DebugLunaHealthManager : MonoBehaviour
{
    void Start()
    {
        var healthMgr = FindObjectOfType<LunaHealthManager>();
        if (healthMgr != null)
        {
            var sr = healthMgr.GetComponentInChildren<SpriteRenderer>();
            if (sr != null)
            {
                Debug.Log($"[Debug] LunaHealthManager is linked to SpriteRenderer: {sr.name} on {sr.gameObject}");
            }
            else
            {
                Debug.LogWarning("[Debug] LunaHealthManager did not find a SpriteRenderer!");
            }
        }
    }
}
