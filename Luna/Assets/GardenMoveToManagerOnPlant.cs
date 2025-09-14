using UnityEngine;

/// Put this on each Garden object (tag "Garden" optional).
/// When a child tagged "Sprout" is planted, this garden is reparented
/// under your persistent manager so it survives scene loads.
[DisallowMultipleComponent]
public class GardenMoveToManagerOnPlant : MonoBehaviour
{
    [Header("Manager to parent under")]
    [Tooltip("Exact name of your persistent manager object (e.g. 'GameManagers XXXXX').")]
    public string managerObjectName = "GameManagers XXXXX";

    [Tooltip("Optional tag to find the manager instead of by name. Leave empty to use name.")]
    public string managerTag = "";

    [Header("Behavior")]
    [Tooltip("Tag used by your flowers/sprouts.")]
    public string sproutTag = "Sprout";

    [Tooltip("If true, move back to original parent when the garden is empty again.")]
    public bool moveBackWhenEmpty = false;

    [Tooltip("Print what this script is doing.")]
    public bool debugLogs = false;

    private Transform _originalParent;
    private Transform _manager;
    private bool _isUnderManager;

    void Awake()
    {
        _originalParent = transform.parent;
        _manager = FindManager();
    }

    void OnEnable()
    {
        // In case a flower is already planted when enabling
        EvaluateAndMove();
    }

    // Fires whenever children are added/removed (plant/pickup)
    void OnTransformChildrenChanged()
    {
        EvaluateAndMove();
    }

    private void EvaluateAndMove()
    {
        bool hasSprout = HasDirectChildWithTag(sproutTag);

        if (hasSprout && !_isUnderManager)
        {
            if (_manager == null) _manager = FindManager();
            if (_manager != null)
            {
                if (debugLogs) Debug.Log($"[Garden] '{name}' → parent under '{_manager.name}'");
                transform.SetParent(_manager, true); // keep world transform
                _isUnderManager = true;
            }
            else if (debugLogs)
            {
                Debug.LogWarning("[Garden] No manager found to move under.");
            }
        }
        else if (!hasSprout && _isUnderManager && moveBackWhenEmpty)
        {
            if (debugLogs) Debug.Log($"[Garden] '{name}' → move back to original parent");
            transform.SetParent(_originalParent, true);
            _isUnderManager = false;
        }
    }

    private bool HasDirectChildWithTag(string tagName)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i);
            if (c != null && c.CompareTag(tagName))
                return true;
        }
        return false;
    }

    private Transform FindManager()
    {
        GameObject go = null;

        if (!string.IsNullOrEmpty(managerTag))
            go = GameObject.FindGameObjectWithTag(managerTag);

        if (go == null && !string.IsNullOrEmpty(managerObjectName))
            go = GameObject.Find(managerObjectName);

        if (go == null && debugLogs)
            Debug.LogWarning($"[Garden] Could not find manager by tag '{managerTag}' or name '{managerObjectName}'.");

        return go ? go.transform : null;
    }
}
