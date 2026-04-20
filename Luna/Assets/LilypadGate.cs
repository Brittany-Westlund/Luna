using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LilypadGate : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LilyStool lilyStool;
    [SerializeField] private OpenBookTrigger openBookTrigger;
    [SerializeField] private WaterDropPromptInteractor waterDropPromptInteractor;

    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";

    [Header("Audio")]
    [SerializeField] private AudioClip lilypadStepSFX;
    [SerializeField] private AudioSource audioSource; // optional

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;

        Debug.Log($"[LilypadGate] Awake on {name}. Trigger={col.isTrigger}");

        if (lilyStool == null)
        {
            Debug.LogWarning($"[LilypadGate] No LilyStool assigned on {name}");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[LilypadGate] OnTriggerEnter2D with: {other.name}, tag={other.tag}");

        if (!other.CompareTag(playerTag))
        {
            Debug.Log($"[LilypadGate] Ignored because tag was not {playerTag}");
            return;
        }

        Debug.Log($"[LilypadGate] PLAYER ENTERED {name}");

        PlayStepSFX();
        SetGateState(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"[LilypadGate] OnTriggerExit2D with: {other.name}, tag={other.tag}");

        if (!other.CompareTag(playerTag))
        {
            Debug.Log($"[LilypadGate] Ignored exit because tag was not {playerTag}");
            return;
        }

        Debug.Log($"[LilypadGate] PLAYER EXITED {name}");
        SetGateState(false);
    }

    private void SetGateState(bool isActive)
    {
        Debug.Log($"[LilypadGate] SetGateState({isActive})");

        if (lilyStool != null)
        {
            lilyStool.SetPlayerOnLilypad(isActive);
        }

        if (waterDropPromptInteractor != null)
        {
            waterDropPromptInteractor.SetPlayerOnLilypad(isActive);
        }

        if (openBookTrigger != null)
        {
            openBookTrigger.enabled = isActive;

            if (!isActive)
            {
                openBookTrigger.ForceClose();
            }
        }
    }

    private void PlayStepSFX()
    {
        if (lilypadStepSFX == null)
        {
            Debug.LogWarning("[LilypadGate] No lilypadStepSFX assigned");
            return;
        }

        if (audioSource != null)
        {
            audioSource.PlayOneShot(lilypadStepSFX);
        }
        else
        {
            AudioSource.PlayClipAtPoint(lilypadStepSFX, transform.position);
        }
    }
}