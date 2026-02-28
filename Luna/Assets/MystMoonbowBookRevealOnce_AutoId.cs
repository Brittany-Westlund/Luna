using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MystMoonbowBookRevealOnce_AutoId : MonoBehaviour
{
    [Header("Auto-generated; do not edit")]
    [SerializeField] private string locationId;

    [Header("Optional")]
    public string playerTag = "Player";
    public float pollInterval = 0.1f;

    private MystRestTransitionAuto mist;
    private BookPageController book;
    private bool firedThisSession;

#if UNITY_EDITOR
    void OnValidate()
    {
        // We only want per-instance IDs in scenes, not on the prefab asset itself.
        if (PrefabUtility.IsPartOfPrefabAsset(gameObject)) return;

        // If this is a prefab instance in a scene (or a normal scene object), give it a GUID once.
        if (string.IsNullOrEmpty(locationId))
        {
            locationId = System.Guid.NewGuid().ToString("N");
            EditorUtility.SetDirty(this);
        }
    }
#endif

    void Awake()
    {
        // No dragging needed
        mist = GetComponent<MystRestTransitionAuto>();
        if (mist == null)
        {
            enabled = false;
            return;
        }

        // Safety: in builds, OnValidate doesn't run; if somehow empty, generate at runtime.
        // (Note: runtime-generated IDs won't persist across edits, but will still work for a built game session.
        // In practice, your scene will have the serialized ID from the editor.)
        if (string.IsNullOrEmpty(locationId))
            locationId = System.Guid.NewGuid().ToString("N");
    }

    void Start()
    {
        InvokeRepeating(nameof(Poll), 0f, pollInterval);
    }

    void Poll()
    {
        if (firedThisSession) return;

        // Consider "moonbow active" as: renderer GO active and alpha visible-ish
        if (mist.moonbowRenderer == null) return;
        if (!mist.moonbowRenderer.gameObject.activeInHierarchy) return;
        if (mist.moonbowRenderer.color.a < 0.15f) return;

        // Find book controller once
        if (book == null)
        {
            var luna = GameObject.FindGameObjectWithTag(playerTag);
            if (luna != null)
                book = luna.GetComponentInChildren<BookPageController>(true);
        }
        if (book == null) return;

        // This method already enforces: "this locationId can never reveal again"
        book.RevealNextFromLocation(locationId);

        firedThisSession = true;
        CancelInvoke(nameof(Poll));
    }
}