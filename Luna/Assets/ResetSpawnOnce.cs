// NukeSavesOnce.cs
using UnityEngine;
using System.IO;

public class NukeSavesOnce : MonoBehaviour
{
    [Tooltip("Delete *.json under Application.persistentDataPath too.")]
    public bool deleteJsonFiles = true;

    [Tooltip("Also delete common save extensions (*.save, *.dat).")]
    public bool deleteCommonSaveFiles = true;

    void Start()
    {
        // 1) PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 2) Files on disk (persistentDataPath)
        string root = Application.persistentDataPath;
        if (deleteJsonFiles)         DeleteByPattern(root, "*.json");
        if (deleteCommonSaveFiles) { DeleteByPattern(root, "*.save"); DeleteByPattern(root, "*.dat"); }

        Debug.Log($"🧹 Wiped PlayerPrefs and save files in: {root}");
        Destroy(gameObject); // run once, then remove itself
    }

    static void DeleteByPattern(string folder, string pattern)
    {
        if (!Directory.Exists(folder)) return;
        var files = Directory.GetFiles(folder, pattern, SearchOption.AllDirectories);
        foreach (var f in files)
        {
            try { File.Delete(f); }
            catch (System.Exception e) { Debug.LogWarning($"Couldn't delete {f}: {e.Message}"); }
        }
    }
}
