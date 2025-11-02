using MoreMountains.CorgiEngine;
using UnityEngine;

public class ResetLadderStateOnLoad : MonoBehaviour
{
    private void OnEnable()
    {
        // --- Reset motion / physics ---
        var controller = GetComponent<CorgiController>();
        if (controller != null)
        {
            controller.SetForce(Vector2.zero);
            controller.GravityActive(true);
            controller.CollisionsOn();     // ensure collisions restored
            controller.ResetParameters();  // clears edge / slope / ladder flags
        }

        // --- Re-permit abilities (prevents stuck states) ---
        var abilities = GetComponents<CharacterAbility>();
        foreach (var ability in abilities)
        {
            if (ability != null)
            {
                ability.AbilityPermitted = true;
                ability.enabled = true;    // force Unity enable
            }
        }

        // --- Optional: re-enable animator ---
        var animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            animator.Rebind();   // resets all states
            animator.Update(0f);
        }

        Debug.Log("[ResetLadderStateOnLoad] Character motion & abilities reset after scene load.");
    }
}
