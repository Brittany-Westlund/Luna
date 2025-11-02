using UnityEngine;
using System.Collections;

public class LunaAnimatorToggle : MonoBehaviour
{
    [Header("Animator Settings")]
    public Animator animator; // The animator on Luna (usually on ModelContainer)
    public RuntimeAnimatorController normalController;
    public RuntimeAnimatorController glowController;

    [Header("Input Settings")]
    public KeyCode toggleKey = KeyCode.G;
    public float holdDuration = 0.7f; // seconds to hold before toggling

    private bool isGlowForm = false;
    private float holdTimer = 0f;

    void Update()
    {
        if (Input.GetKey(toggleKey))
        {
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdDuration)
            {
                ToggleAnimator();
                holdTimer = 0f;
            }
        }

        if (Input.GetKeyUp(toggleKey))
            holdTimer = 0f;
    }

    private void ToggleAnimator()
    {
        if (animator == null) return;

        isGlowForm = !isGlowForm;
        animator.runtimeAnimatorController = isGlowForm ? glowController : normalController;

        Debug.Log($"✨ Luna form toggled: {(isGlowForm ? "GlowForm" : "NormalForm")}");
    }

    public bool IsGlowAnimatorActive()
    {
        // Safe checks; returns true only when GlowForm controller is the current one
        return animator != null 
            && glowController != null 
            && animator.runtimeAnimatorController == glowController;
    }
}
