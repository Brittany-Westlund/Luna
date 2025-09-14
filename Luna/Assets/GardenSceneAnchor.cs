// add to each persistent Garden
using UnityEngine;
using UnityEngine.SceneManagement;

public class GardenSceneAnchor : MonoBehaviour
{
    public string anchorNameOrKey;   // e.g., "Meadow-G1"
    public bool snapToAnchor = true;

    void OnEnable() { SceneManager.sceneLoaded += OnLoaded; TryAnchor(); }
    void OnDisable(){ SceneManager.sceneLoaded -= OnLoaded; }

    void OnLoaded(Scene s, LoadSceneMode m) => TryAnchor();

    void TryAnchor()
    {
        string target = string.IsNullOrEmpty(anchorNameOrKey) ? name : anchorNameOrKey;
        var all = Object.FindObjectsOfType<Transform>(includeInactive: true);
        Transform anchor = null;
        foreach (var t in all) { if (t.hideFlags==HideFlags.None && t.name==target) { anchor = t; break; } }

        bool shouldBeActive = anchor != null;
        gameObject.SetActive(shouldBeActive);

        if (shouldBeActive && snapToAnchor)
        {
            transform.SetParent(anchor, true);
            transform.position = anchor.position;
            transform.rotation = anchor.rotation;
        }
    }
}
