using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class BookPageHistory
{
    [Serializable]
    private class SaveData
    {
        public List<string> ids = new List<string>();
    }

    private static List<string> visitedIDs;

    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, "bookpages.json");

    public static IReadOnlyList<string> VisitedPages
    {
        get
        {
            EnsureLoaded();
            return visitedIDs.AsReadOnly();
        }
    }

    public static bool HasVisited(string id)
    {
        EnsureLoaded();
        return visitedIDs.Contains(id);
    }

    public static void MarkVisited(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        EnsureLoaded();

        if (visitedIDs.Contains(id))
            return;

        visitedIDs.Add(id);
        Save();
    }

    private static void EnsureLoaded()
    {
        if (visitedIDs != null)
            return;

        Load();
    }

    public static void Load()
    {
        visitedIDs = new List<string>();

        try
        {
            if (!File.Exists(SavePath))
                return;

            string json = File.ReadAllText(SavePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data != null && data.ids != null)
                visitedIDs = data.ids;

            Debug.Log(
                $"[BookPageHistory] Loaded {visitedIDs.Count} pages."
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"[BookPageHistory] Load failed: {e}"
            );
        }
    }

    public static void Save()
    {
        EnsureLoaded();

        try
        {
            SaveData data = new SaveData
            {
                ids = visitedIDs
            };

            File.WriteAllText(
                SavePath,
                JsonUtility.ToJson(data, true)
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"[BookPageHistory] Save failed: {e}"
            );
        }
    }

    public static void ResetAll()
    {
        visitedIDs = new List<string>();

        try
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);

            Debug.Log("[BookPageHistory] History reset.");
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"[BookPageHistory] Reset failed: {e}"
            );
        }
    }
}