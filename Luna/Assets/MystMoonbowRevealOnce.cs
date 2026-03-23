using UnityEngine;

public class MystMoonbowRevealOnce : MonoBehaviour
{
    [Header("Reveal ID")]
    [SerializeField] private string revealId = "MarySpread1";

    [Header("Detection")]
    [SerializeField] private float alphaThreshold = 0.15f;
    [SerializeField] private float pollInterval = 0.1f;

    [Header("Optional Dialogue Termination")]
    [SerializeField] private string entryIDToTerminate;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private MystRestTransitionAuto mistTransition;
    private BookControllerSimple bookSimple;
    private LevelDialogueManager levelDialogueManager;

    private float nextPollTime;
    private bool didReveal;

    private void Awake()
    {
        AutoWire();
    }

    private void Update()
    {
        if (didReveal) return;

        if (Time.time < nextPollTime) return;
        nextPollTime = Time.time + pollInterval;

        if (mistTransition == null)
            mistTransition = GetComponent<MystRestTransitionAuto>();

        if (bookSimple == null || levelDialogueManager == null)
            AutoWire();

        if (mistTransition == null || bookSimple == null)
            return;

        var moonbowRenderer = mistTransition.moonbowRenderer;
        if (moonbowRenderer == null) return;
        if (!moonbowRenderer.gameObject.activeInHierarchy) return;
        if (moonbowRenderer.color.a < alphaThreshold) return;

        bool success = bookSimple.RevealNextFromLocation(revealId);

        if (debugLogs)
        {
            Debug.Log($"📖 {name}: Reveal attempt success={success}, revealId={revealId}");
        }

        if (!success) return;

        didReveal = true;

        if (!string.IsNullOrWhiteSpace(entryIDToTerminate) && levelDialogueManager != null)
        {
            levelDialogueManager.MarkTerminated(entryIDToTerminate);

            if (debugLogs)
                Debug.Log($"📖 {name}: Terminated '{entryIDToTerminate}'");
        }

        enabled = false;
    }

    private void AutoWire()
    {
        // Find mist transition locally
        if (mistTransition == null)
            mistTransition = GetComponent<MystRestTransitionAuto>();

        // 🔥 Find ClosedBookTiny specifically
        if (bookSimple == null)
        {
            GameObject bookObj = GameObject.Find("ClosedBookTiny");

            if (bookObj != null)
            {
                bookSimple = bookObj.GetComponent<BookControllerSimple>();

                if (debugLogs)
                    Debug.Log($"📖 Found ClosedBookTiny: {bookObj.name}");
            }
            else if (debugLogs)
            {
                Debug.LogWarning("📖 Could not find GameObject named 'ClosedBookTiny'");
            }
        }

        // 🔥 Find LevelDialogueManager anywhere in scene
        if (levelDialogueManager == null)
        {
            levelDialogueManager = FindFirstObjectByTypeCompat<LevelDialogueManager>();

            if (debugLogs && levelDialogueManager != null)
                Debug.Log($"📖 Found LevelDialogueManager: {levelDialogueManager.name}");
        }
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