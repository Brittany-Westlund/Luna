using UnityEngine;

// Persistent across scene changes and game launches, independent of BookPageHistory.
public static class FlorilegiumProgress
{
    private const string Prefix = "AcrossMoonbows.Florilegium.";
    private const string FirstVisitKey = Prefix + "FirstVisitCompleted";

    public static bool FirstVisitCompleted => PlayerPrefs.GetInt(FirstVisitKey, 0) == 1;

    public static void MarkFirstVisitCompleted()
    {
        PlayerPrefs.SetInt(FirstVisitKey, 1);
        PlayerPrefs.Save();
    }

    // Distinct from page unlocking: a one-time ribbon action is completed once
    // even if its spread remains readable on later book visits.
    public static bool IsRibbonActionCompleted(string actionId)
    {
        return !string.IsNullOrWhiteSpace(actionId) &&
            PlayerPrefs.GetInt(Prefix + "Ribbon." + actionId.Trim(), 0) == 1;
    }

    public static void MarkRibbonActionCompleted(string actionId)
    {
        if (string.IsNullOrWhiteSpace(actionId)) return;
        PlayerPrefs.SetInt(Prefix + "Ribbon." + actionId.Trim(), 1);
        PlayerPrefs.Save();
    }

    public static bool IsUnlocked(string pageId)
    {
        if (string.IsNullOrWhiteSpace(pageId)) return false;
        return PlayerPrefs.GetInt(Prefix + "Page." + pageId.Trim(), 0) == 1;
    }

    public static void Unlock(string pageId)
    {
        if (string.IsNullOrWhiteSpace(pageId))
        {
            Debug.LogWarning("[Florilegium] Cannot unlock a page with an empty ID.");
            return;
        }

        PlayerPrefs.SetInt(Prefix + "Page." + pageId.Trim(), 1);
        PlayerPrefs.Save();
    }
}
