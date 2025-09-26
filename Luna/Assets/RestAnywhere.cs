using UnityEngine;

[RequireComponent(typeof(LunaRest))]
public class RestAnywhere : MonoBehaviour
{
    public KeyCode restKey = KeyCode.Z;
    public bool holdToRest = false;
    public bool enableAnywhere = true;

    private LunaRest rest;

    void Awake()
    {
        rest = GetComponent<LunaRest>();
    }

    void Update()
    {
        if (rest == null) return;

        // 🚫 If we're in a Garden, DO NOTHING here.
        // Let LunaRest handle input in gardens to avoid double-toggling.
        if (rest.isInGarden) return;

        if (!enableAnywhere) return;

        if (holdToRest)
        {
            if (Input.GetKeyDown(restKey) && !rest.isResting) rest.BeginRestExternal();
            if (Input.GetKeyUp(restKey)   &&  rest.isResting) rest.EndRestExternal();
        }
        else
        {
            if (Input.GetKeyDown(restKey))
            {
                if (rest.isResting) rest.EndRestExternal();
                else                rest.BeginRestExternal();
            }
        }
    }

    private void TryStartRest()
    {
        // Start if in a garden OR anywhere is enabled
        if (rest.isResting) return;
        if (rest.isInGarden || enableAnywhere)
            rest.BeginRestExternal();
    }

    private void TryStopRest()
    {
        if (rest.isResting)
            rest.EndRestExternal();
    }
}
