using UnityEngine;

public class LunaFacingResetter : MonoBehaviour
{
    [Header("Facing Sprites (Children)")]
    public GameObject faceLeftObject;
    public GameObject faceRightObject;

    [Header("Luna’s Main Sprite")]
    public SpriteRenderer lunaRenderer;

    void Start()
    {
        // Disable both facing sprites, just to be safe
        if (faceLeftObject != null)  faceLeftObject.SetActive(false);
        if (faceRightObject != null) faceRightObject.SetActive(false);

        // Enable Luna’s main sprite
        if (lunaRenderer != null) lunaRenderer.enabled = true;
        else Debug.LogWarning("LunaFacingResetter: SpriteRenderer not assigned.");
    }
}
