using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;

public class SceneTransitionLogger : MonoBehaviour, MMEventListener<CorgiEngineEvent>
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnLoaded;
        SceneManager.activeSceneChanged += OnActiveChanged;
        this.MMEventStartListening<CorgiEngineEvent>();
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLoaded;
        SceneManager.activeSceneChanged -= OnActiveChanged;
        this.MMEventStopListening<CorgiEngineEvent>();
    }

    void OnLoaded(Scene s, LoadSceneMode m)
        => Debug.Log($"🧭 sceneLoaded → {s.name} ({m}) @ {Time.time:F2}s");

    void OnActiveChanged(Scene from, Scene to)
        => Debug.Log($"🧭 activeSceneChanged → {from.name} ➜ {to.name} @ {Time.time:F2}s");

    public void OnMMEvent(CorgiEngineEvent e)
        => Debug.Log($"🎮 CorgiEvent: {e.EventType} @ {Time.time:F2}s");
}
