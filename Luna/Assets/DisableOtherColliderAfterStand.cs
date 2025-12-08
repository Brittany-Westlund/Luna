using System.Collections;
using UnityEngine;

public class DisableOtherColliderAfterStand : MonoBehaviour
{
    [Header("Target")]
    public Collider2D colliderToDisable;

    [Header("Timing")]
    public float standTimeRequired = 1f;

    [Header("Detection")]
    public string playerTag = "Player";

    private Coroutine standRoutine;
    private bool hasDisabled = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag(playerTag)) return;

        // Start stand timer
        if (standRoutine == null)
            standRoutine = StartCoroutine(StandTimer());
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag(playerTag)) return;

        // Cancel timer if she steps off early
        if (standRoutine != null)
        {
            StopCoroutine(standRoutine);
            standRoutine = null;
        }

        // Re-enable when she leaves
        if (hasDisabled && colliderToDisable != null)
        {
            colliderToDisable.enabled = true;
            hasDisabled = false;
        }
    }

    private IEnumerator StandTimer()
    {
        yield return new WaitForSeconds(standTimeRequired);

        if (colliderToDisable != null)
        {
            colliderToDisable.enabled = false;
            hasDisabled = true;
        }

        standRoutine = null;
    }
}
