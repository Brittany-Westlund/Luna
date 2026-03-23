using UnityEngine;

public class MoonbowSpreadUnlockTrigger : MonoBehaviour
{
    [Header("Spread Unlock")]
    [SerializeField] private string revealId = "MarySpread1";

    [Header("OpenBook Detection")]
    [SerializeField] private SpriteRenderer openBookRenderer;
    [SerializeField] private string openBookChildName = "OpenBook";

    [Header("Moonbow Detection")]
    [SerializeField] private SpriteRenderer moonbowRenderer;
    [SerializeField] private string moonbowChildName = "Moonbow";
    [SerializeField] private float alphaThreshold = 0.15f;

    [Header("Polling")]
    [SerializeField] private float pollInterval = 0.1f;

    [Header("Optional Dialogue Termination")]
    [SerializeField] private string entryIDToTerminate;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private BookControllerSimple bookSimple;
    private LevelDialogueManager levelDialogueManager;

    private float nextPollTime;
    private bool finished;

    private void Awake()
    {
        AutoWire();
    }

    private void Start()
    {
        if (bookSimple != null && bookSimple.HasUsedRevealId(revealId))
        {
            finished = true;

            if (debugLogs)
                Debug.Log($"📖 {name}: revealId already used, disabling trigger -> {revealId}");

            enabled = false;
        }
    }

    private void Update()
    {
        if (finished) return;

        if (Time.time < nextPollTime) return;
        nextPollTime = Time.time + pollInterval;

        if (openBookRenderer == null || moonbowRenderer == null || bookSimple == null || levelDialogueManager == null)
            AutoWire();

        if (openBookRenderer == null || moonbowRenderer == null || bookSimple == null)
            return;

        // The book must actually be open in the world.
        if (!IsRendererVisible(openBookRenderer))
            return;

        // The actual reveal visual must be present.
        // Mist/silvermist should NOT count unless this renderer is the real moonbow.
        if (!IsRendererVisible(moonbowRenderer))
            return;

        // The UI must be open to the blank revealable page right now.
        if (!bookSimple.IsShowingRevealableBlankPage())
        {
            if (debugLogs)
                Debug.Log($"📖 {name}: Moonbow + OpenBook visible, but UI is not on the revealable blank page yet.");

            return;
        }

        bool success = bookSimple.RevealNextFromLocation(revealId);

        if (debugLogs)
            Debug.Log($"📖 {name}: Reveal attempt success={success}, revealId={revealId}");

        if (!success)
        {
            if (bookSimple.HasUsedRevealId(revealId))
            {
                finished = true;

                if (debugLogs)
                    Debug.Log($"📖 {name}: revealId already consumed; disabling trigger.");

                enabled = false;
            }

            return;
        }

        finished = true;

        if (!string.IsNullOrWhiteSpace(entryIDToTerminate) && levelDialogueManager != null)
        {
            levelDialogueManager.MarkTerminated(entryIDToTerminate);

            if (debugLogs)
                Debug.Log($"📖 {name}: Terminated dialogue entry '{entryIDToTerminate}'");
        }

        enabled = false;
    }

    private void AutoWire()
    {
        if (openBookRenderer == null)
            openBookRenderer = FindLocalRendererByName(openBookChildName);

        if (moonbowRenderer == null)
            moonbowRenderer = FindLocalRendererByName(moonbowChildName);

        if (bookSimple == null)
            bookSimple = FindFirstObjectByTypeCompat<BookControllerSimple>();

        if (levelDialogueManager == null)
            levelDialogueManager = FindFirstObjectByTypeCompat<LevelDialogueManager>();
    }

    private SpriteRenderer FindLocalRendererByName(string childName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < children.Length; i++)
        {
            Transform t = children[i];
            if (t == null) continue;
            if (t.name != childName) continue;

            SpriteRenderer sr = t.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (debugLogs)
                    Debug.Log($"📖 {name}: Found renderer '{childName}' on '{t.name}'");

                return sr;
            }
        }

        if (debugLogs)
            Debug.LogWarning($"📖 {name}: Could not find child named '{childName}' with SpriteRenderer.");

        return null;
    }

    private bool IsRendererVisible(SpriteRenderer sr)
    {
        if (sr == null) return false;
        if (!sr.enabled) return false;
        if (!sr.gameObject.activeInHierarchy) return false;
        if (sr.color.a < alphaThreshold) return false;

        return true;
    }

    private static T FindFirstObjectByTypeCompat<T>() where T : Object
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindFirstObjectByType<T>();
#else
        return Object.FindObjectOfType<T>();
#endif
    }
}