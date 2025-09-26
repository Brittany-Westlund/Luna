// ThoughtBubbleManager.cs
using UnityEngine;

public class ThoughtBubbleManager : MonoBehaviour
{
    public static ThoughtBubbleManager Instance;

    private string _localKey;       // set while inside a garden
    private string _checkpointKey;  // set by checkpoints

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    // Gardens
    public void SetLocalKey(string key) => _localKey = key;
    public void ClearLocalKey()         => _localKey = null;

    // Checkpoints
    public void SetCheckpointKey(string key) => _checkpointKey = key;

    // Resolve priority: Garden > Checkpoint > Fallback
    public string ResolveKey(string fallback)
    {
        if (!string.IsNullOrEmpty(_localKey))      return _localKey;
        if (!string.IsNullOrEmpty(_checkpointKey)) return _checkpointKey;
        return fallback;
    }
}
