// HideSpriteRenderersInEditor.cs
using UnityEngine;

[ExecuteAlways]
public class HideSpriteRenderersInEditor : MonoBehaviour
{
    void OnEnable()
    {
        if (!Application.isPlaying)
        {
            foreach (var r in GetComponentsInChildren<SpriteRenderer>(true))
                r.enabled = false;
        }
    }

    // Optional: if you tweak the prefab while selected, keep it hidden
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            foreach (var r in GetComponentsInChildren<SpriteRenderer>(true))
                r.enabled = false;
        }
    }
}
