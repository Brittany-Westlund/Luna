using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;

public class LightBarModifier : MonoBehaviour
{
    public MMProgressBar lightBar;

    public float speedMultiplier = 1.1f;
    public float jumpMultiplier = 1.1f;

    private CharacterHorizontalMovement characterHorizontalMovement;
    private CharacterJump characterJump;
    private float originalWalkSpeed;
    private float originalJumpHeight;
    private bool isEnhanced = false;

    private void Start()
    {
        characterHorizontalMovement = GetComponent<CharacterHorizontalMovement>();
        characterJump = GetComponent<CharacterJump>();

        if (characterHorizontalMovement != null)
        {
            originalWalkSpeed = characterHorizontalMovement.WalkSpeed;
        }

        if (characterJump != null)
        {
            originalJumpHeight = characterJump.JumpHeight;
        }
    }

    private void Update()
    {
        if (lightBar == null) return;

        float fillAmount = lightBar.BarProgress; // Using BarProgress property

        if (fillAmount > 0.1f && !isEnhanced)
        {
            ApplyEnhancements();
        }
        else if (fillAmount <= 0.1f && isEnhanced)
        {
            RestoreOriginalValues();
        }
    }

    private void ApplyEnhancements()
    {
        if (characterHorizontalMovement != null)
        {
            characterHorizontalMovement.WalkSpeed = originalWalkSpeed * speedMultiplier;
        }

        if (characterJump != null)
        {
            characterJump.JumpHeight = originalJumpHeight * jumpMultiplier;
        }

        isEnhanced = true;
    }

    private void RestoreOriginalValues()
    {
        if (characterHorizontalMovement != null)
        {
            characterHorizontalMovement.WalkSpeed = originalWalkSpeed;
        }

        if (characterJump != null)
        {
            characterJump.JumpHeight = originalJumpHeight;
        }

        isEnhanced = false;
    }
}
