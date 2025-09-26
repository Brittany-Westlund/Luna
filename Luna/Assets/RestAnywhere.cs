using UnityEngine;

[RequireComponent(typeof(LunaRest))]
public class RestAnywhere : MonoBehaviour
{
    [Header("Controls")]
    public KeyCode restKey = KeyCode.Z;
    public bool holdToRest = false;     // if true: hold to keep resting; if false: toggle on key press

    [Header("Behavior")]
    public bool enableAnywhere = true;  // allow starting rest outside gardens

    private LunaRest rest;

    void Awake()
    {
        rest = GetComponent<LunaRest>();
    }

    void Update()
    {
        if (rest == null) return;

        if (holdToRest)
        {
            if (Input.GetKeyDown(restKey)) TryStartRest();
            if (Input.GetKeyUp(restKey))   TryStopRest();
        }
        else
        {
            if (Input.GetKeyDown(restKey))
            {
                if (rest.isResting) TryStopRest();
                else                TryStartRest();
            }
        }
        // Note: movement-cancel still handled by LunaRest itself.
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
