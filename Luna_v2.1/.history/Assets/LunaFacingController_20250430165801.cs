using UnityEngine;

/// <summary>
/// This script lets you manually set Luna's starting facing direction (left or right),
/// without interfering with other movement scripts that flip her based on input or position.
/// Attach this to Luna. It applies the direction once at Start.
/// </summary>
public class LunaFacingController : MonoBehaviour
{
    [Tooltip("Check this if you want Luna to start facing right. Uncheck to face left.")]
    public bool faceRight = false;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;

        // Determine facing direction based on scale.x
        float direction = faceRight ? 1f : -1f;
        transform.localScale = new Vector3(
            Mathf.Abs(originalScale.x) * direction,
            originalScale.y,
            originalScale.z
        );
    }
}
