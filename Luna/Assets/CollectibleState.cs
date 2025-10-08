using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor; // for the custom button
#endif

/// <summary>
/// 🌿 Your magical notebook — remembers every collectible the player has picked up.
/// Uses Unity’s built-in JsonUtility (no external libraries).
/// </summary>
[CreateAssetMenu(fileName = "CollectibleState", menuName = "CharmLab/Collectible State")]
public class CollectibleState : ScriptableObject
{
    [SerializeField] private List<string> collectedIDs = new();   // Saved IDs (visible for debugging)
    private HashSet<string> collectedSet;                         // Fast lookup version of that list

    // 📁 Save location on disk
    private string SavePath => Path.Combine(Application.persistentDataPath, "collectibles.json");

    private void OnEnable()
    {
        collectedSet = new HashSet<string>(collectedIDs);
        Load(); // load data when this asset wakes up
    }

    // 🌼 Check if an item with this ID is already collected
    public bool HasCollected(string id) => collectedSet.Contains(id);

    // ✏️ Mark a collectible as picked up
    public void MarkCollected(string id)
    {
        if (collectedSet.Add(id))
        {
            collectedIDs.Add(id);
            Save();
        }
    }

    // 🧹 Clear everything (used by the editor button or a "New Game")
    public void ResetAll()
    {
        collectedIDs.Clear();
        collectedSet.Clear();
        Save();
    }

    // ---------- SAVE / LOAD using Unity’s built-in JsonUtility ----------

    [System.Serializable]
    private class SaveData
    {
        public List<string> ids;
    }

    // 💾 Write the list to a JSON file
    public void Save()
    {
        try
        {
            var data = new SaveData { ids = collectedIDs };
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"[CollectibleState] Saved {collectedIDs.Count} IDs → {SavePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CollectibleState] Save failed: {e}");
        }
    }

    // 📖 Read the file back
    public void Load()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log("[CollectibleState] No save file found; starting fresh.");
                return;
            }

            var json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<SaveData>(json);
            collectedIDs = data?.ids ?? new();
            collectedSet = new HashSet<string>(collectedIDs);
            Debug.Log($"[CollectibleState] Loaded {collectedIDs.Count} IDs from {SavePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CollectibleState] Load failed: {e}");
        }
    }
}

#if UNITY_EDITOR
// 🧰 Adds a handy reset button in the Inspector
[CustomEditor(typeof(CollectibleState))]
public class CollectibleStateEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        CollectibleState state = (CollectibleState)target;

        EditorGUILayout.Space();
        if (GUILayout.Button("🧹 Reset Collectibles (Clear Save File)"))
        {
            state.ResetAll();

            // also delete the file on disk
            string path = Path.Combine(Application.persistentDataPath, "collectibles.json");
            if (File.Exists(path)) File.Delete(path);

            EditorUtility.SetDirty(state);
            Debug.Log("[CollectibleState] Save file cleared and collectible list reset.");
        }
    }
}
#endif
