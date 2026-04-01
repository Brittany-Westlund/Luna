using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SporeSpawnSourceDebugger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LunaSporeSystem lunaSporeSystem;

    [Header("Logging")]
    [SerializeField] private bool logOnSporeSpawn = true;
    [SerializeField] private bool logOnSporeDespawn = false;
    [SerializeField] private bool dumpAllScriptsOnLuna = true;
    [SerializeField] private bool dumpAllScriptsInScene = false;
    [SerializeField] private bool logNearbyChecks = true;
    [SerializeField] private bool logHeldKeys = true;

    [Header("Held Key Log Throttle")]
    [SerializeField] private float heldKeyLogInterval = 0.25f;

    private FieldInfo activeSporeField;
    private GameObject lastActiveSpore;

    private float nextRLogTime;
    private float nextSLogTime;

    private void Reset()
    {
        if (lunaSporeSystem == null)
        {
            lunaSporeSystem = GetComponent<LunaSporeSystem>();
        }
    }

    private void Awake()
    {
        if (lunaSporeSystem == null)
        {
            lunaSporeSystem = GetComponent<LunaSporeSystem>();
        }

        if (lunaSporeSystem == null)
        {
            Debug.LogError("[SporeSpawnSourceDebugger] No LunaSporeSystem found.", this);
            enabled = false;
            return;
        }

        activeSporeField = typeof(LunaSporeSystem).GetField("activeSpore", BindingFlags.NonPublic | BindingFlags.Instance);

        if (activeSporeField == null)
        {
            Debug.LogError("[SporeSpawnSourceDebugger] Could not find private field 'activeSpore' on LunaSporeSystem.", this);
            enabled = false;
            return;
        }

        lastActiveSpore = GetActiveSpore();

        Debug.Log("[SporeSpawnSourceDebugger] Ready. Watching activeSpore for changes.", this);
    }

    private void Update()
    {
        LogHeldKeysIfNeeded();

        GameObject currentActiveSpore = GetActiveSpore();

        if (currentActiveSpore != lastActiveSpore)
        {
            if (currentActiveSpore != null)
            {
                if (logOnSporeSpawn)
                {
                    LogSpawnEvent(currentActiveSpore);
                }
            }
            else
            {
                if (logOnSporeDespawn)
                {
                    Debug.Log("[SporeSpawnSourceDebugger] activeSpore changed to NULL.", this);
                }
            }

            lastActiveSpore = currentActiveSpore;
        }
    }

    private void LogHeldKeysIfNeeded()
    {
        if (!logHeldKeys) return;

        if (Input.GetKey(KeyCode.R) && Time.time >= nextRLogTime)
        {
            nextRLogTime = Time.time + heldKeyLogInterval;
            Debug.Log("[SporeSpawnSourceDebugger] R is currently HELD.", this);
        }

        if (Input.GetKey(KeyCode.S) && Time.time >= nextSLogTime)
        {
            nextSLogTime = Time.time + heldKeyLogInterval;
            Debug.Log("[SporeSpawnSourceDebugger] S is currently HELD.", this);
        }
    }

    private void LogSpawnEvent(GameObject spawnedSpore)
    {
        bool sDown = Input.GetKeyDown(KeyCode.S);
        bool sHeld = Input.GetKey(KeyCode.S);
        bool rDown = Input.GetKeyDown(KeyCode.R);
        bool rHeld = Input.GetKey(KeyCode.R);

        bool nearWildSpore = InvokePrivateBool("IsNearWildSpore");
        bool nearSprout = InvokePrivateBool("IsNearSprout");

        string header =
            "[SporeSpawnSourceDebugger] SPORE SPAWN DETECTED\n" +
            "- Spawned object: " + spawnedSpore.name + "\n" +
            "- Frame: " + Time.frameCount + "\n" +
            "- Scene: " + SceneManager.GetActiveScene().name + "\n" +
            "- S down: " + sDown + "\n" +
            "- S held: " + sHeld + "\n" +
            "- R down: " + rDown + "\n" +
            "- R held: " + rHeld;

        if (logNearbyChecks)
        {
            header +=
                "\n- IsNearWildSpore(): " + nearWildSpore +
                "\n- IsNearSprout(): " + nearSprout;
        }

        header +=
            "\n- Conclusion: " + BuildConclusion(sDown, sHeld, rDown, rHeld, nearWildSpore, nearSprout);

        Debug.Log(header, this);

        if (dumpAllScriptsOnLuna)
        {
            DumpScriptsOnGameObjectHierarchy(transform.root.gameObject, "[SporeSpawnSourceDebugger] Scripts on Luna/root hierarchy");
        }

        if (dumpAllScriptsInScene)
        {
            DumpAllEnabledScriptsInScene();
        }
    }

    private string BuildConclusion(bool sDown, bool sHeld, bool rDown, bool rHeld, bool nearWildSpore, bool nearSprout)
    {
        if ((rDown || rHeld) && !sDown && !sHeld)
        {
            return "Spore appeared while R was active and S was not. Another system is very likely causing the spawn.";
        }

        if ((sDown || sHeld) && !nearWildSpore && !nearSprout)
        {
            return "Spore appeared while S was active and no nearby state was detected. Possible external call or state timing issue.";
        }

        if ((sDown || sHeld) && (nearWildSpore || nearSprout))
        {
            return "Spore appeared while S was active and gameplay state also changed. Could still be normal logic depending on timing.";
        }

        return "Spore appeared without clear direct S input on this frame. Likely external trigger or timing from another script.";
    }

    private GameObject GetActiveSpore()
    {
        if (activeSporeField == null || lunaSporeSystem == null)
        {
            return null;
        }

        return activeSporeField.GetValue(lunaSporeSystem) as GameObject;
    }

    private bool InvokePrivateBool(string methodName)
    {
        if (lunaSporeSystem == null) return false;

        MethodInfo method = typeof(LunaSporeSystem).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
        if (method == null) return false;

        object result = method.Invoke(lunaSporeSystem, null);
        return result is bool b && b;
    }

    private void DumpScriptsOnGameObjectHierarchy(GameObject root, string label)
    {
        List<string> lines = new List<string>();
        MonoBehaviour[] scripts = root.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour script in scripts)
        {
            if (script == null)
            {
                lines.Add("- MISSING SCRIPT on " + GetHierarchyPath(root.transform));
                continue;
            }

            lines.Add(
                "- " + script.GetType().Name +
                " | enabled=" + script.enabled +
                " | object=" + GetHierarchyPath(script.transform)
            );
        }

        Debug.Log(label + "\n" + string.Join("\n", lines), root);
    }

    private void DumpAllEnabledScriptsInScene()
    {
        MonoBehaviour[] allScripts = FindObjectsOfType<MonoBehaviour>(true);
        List<string> lines = new List<string>();

        foreach (MonoBehaviour script in allScripts)
        {
            if (script == null) continue;
            if (!script.enabled) continue;

            lines.Add(
                "- " + script.GetType().Name +
                " | object=" + GetHierarchyPath(script.transform)
            );
        }

        Debug.Log("[SporeSpawnSourceDebugger] All ENABLED MonoBehaviours in scene:\n" + string.Join("\n", lines), this);
    }

    private string GetHierarchyPath(Transform current)
    {
        if (current == null) return "(null)";

        string path = current.name;
        while (current.parent != null)
        {
            current = current.parent;
            path = current.name + "/" + path;
        }

        return path;
    }
}