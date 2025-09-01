using UnityEngine;

public class LunaFacingVisualSwitcher : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer mainRenderer;        // Luna’s original sprite (will be hidden)
    public GameObject faceLeftObject;          // Child with left-facing sprite
    public GameObject faceRightObject;         // Child with right-facing sprite

    [Header("Facing Direction")]
    public bool faceRight = false;             // Toggle this in Inspector

    void Start()
    {
        // Hide main sprite
        if (mainRenderer != null)
            mainRenderer.enabled = false;

        // Enable the correct facing object
        if (faceLeftObject != null)
            faceLeftObject.SetActive(!faceRight);

        if (faceRightObject != null)
            faceRightObject.SetActive(faceRight);
    }
}
