using UnityEngine;
using PixelCrushers.DialogueSystem;

public class TeacupOutcomeResponder : MonoBehaviour
{
    [Header("SPRITES — Enable On Success")]
    public SpriteRenderer[] enableSpritesOnSuccess;
    public SpriteRenderer[] disableSpritesOnSuccess;

    [Header("SPRITES — Enable On Failure")]
    public SpriteRenderer[] enableSpritesOnFailure;
    public SpriteRenderer[] disableSpritesOnFailure;

    [Header("GAME OBJECTS — Enable On Success")]
    public GameObject[] enableObjectsOnSuccess;
    public GameObject[] disableObjectsOnSuccess;

    [Header("GAME OBJECTS — Enable On Failure")]
    public GameObject[] enableObjectsOnFailure;
    public GameObject[] disableObjectsOnFailure;

    [Header("Dialogue Triggers")]
    public DialogueSystemTrigger successDialogueTrigger;
    public DialogueSystemTrigger wrongTeaDialogueTrigger;

    private bool hasFired = false; // ✅ one-shot lock (expo safety)

    public void HandleTeaOutcome(bool wasCorrect)
    {
        if (hasFired) return;
        hasFired = true;

        if (wasCorrect)
        {
            // ✅ SPRITES
            foreach (var sr in enableSpritesOnSuccess)
                if (sr) sr.enabled = true;

            foreach (var sr in disableSpritesOnSuccess)
                if (sr) sr.enabled = false;

            // ✅ OBJECTS
            foreach (var go in enableObjectsOnSuccess)
                if (go) go.SetActive(true);

            foreach (var go in disableObjectsOnSuccess)
                if (go) go.SetActive(false);

            // ✅ SUCCESS DIALOGUE
            if (successDialogueTrigger)
            {
                successDialogueTrigger.enabled = false;
                successDialogueTrigger.enabled = true; // fires OnEnable
            }
        }
        else
        {
            // ❌ SPRITES
            foreach (var sr in enableSpritesOnFailure)
                if (sr) sr.enabled = true;

            foreach (var sr in disableSpritesOnFailure)
                if (sr) sr.enabled = false;

            // ❌ OBJECTS
            foreach (var go in enableObjectsOnFailure)
                if (go) go.SetActive(true);

            foreach (var go in disableObjectsOnFailure)
                if (go) go.SetActive(false);

            // ❌ WRONG-TEA DIALOGUE
            if (wrongTeaDialogueTrigger)
            {
                wrongTeaDialogueTrigger.enabled = false;
                wrongTeaDialogueTrigger.enabled = true;
            }
        }
    }
}
