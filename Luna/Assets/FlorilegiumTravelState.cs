// Holds a return waypoint for the current trip through the book scene.
// This data is intentionally NOT saved: only page unlocks / first-visit completion persist.
public static class FlorilegiumTravelState
{
    public static string ReturnSceneName { get; private set; }
    public static string ReturnWaypointId { get; private set; }

    public static bool HasReturnDestination =>
        !string.IsNullOrWhiteSpace(ReturnSceneName) &&
        !string.IsNullOrWhiteSpace(ReturnWaypointId);

    public static void BeginVisit(string sceneName, string waypointId)
    {
        ReturnSceneName = sceneName;
        ReturnWaypointId = waypointId;
    }

    public static bool IsReturnFor(string sceneName, string waypointId)
    {
        return HasReturnDestination &&
               ReturnSceneName == sceneName &&
               ReturnWaypointId == waypointId;
    }

    public static void ClearReturn()
    {
        ReturnSceneName = null;
        ReturnWaypointId = null;
    }
}
