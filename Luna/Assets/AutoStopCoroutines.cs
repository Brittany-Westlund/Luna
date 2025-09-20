using UnityEngine;

/// <summary>
/// A wrapper around SpriteRenderer.color that prevents
/// MissingReferenceException when the renderer is destroyed.
/// Attach this to the same GameObject as your SpriteRenderer.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SafeSpriteRenderer : MonoBehaviour
{
    private SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public Color Color
    {
        get
        {
            if (sr == null || sr.Equals(null)) return Color.white;
            return sr.color;
        }
        set
        {
            if (sr == null || sr.Equals(null)) return;
            sr.color = value;
        }
    }
}
