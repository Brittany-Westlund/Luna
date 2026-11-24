using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MoreMountains.CorgiEngine;

// Attach to a lilystool with a 2D trigger collider.
// Stores the originating scene and waypoint, then opens the shared book scene.
// On return, positions the player at Return Position after the level has spawned.
public class FlorilegiumWaypoint : MonoBehaviour
{
    [Header("Waypoint Identity")]
    [SerializeField] private string waypointId = "";
    [SerializeField] private Transform returnPosition;

    [Header("Book Scene")]
    [SerializeField] private string florilegiumSceneName = "Florilegium";
    [SerializeField] private bool useCorgiLevelManager = true;
    [SerializeField] private bool levelManagerFadeOut = true;
    [SerializeField] private bool levelManagerSave = true;

    [Header("Interaction")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private Graphic promptGraphic;
    [SerializeField] private CanvasGroup promptCanvasGroup;

    [Header("Return")]
    [Tooltip("The tag of Luna's root GameObject, also used when restoring her position.")]
    [SerializeField] private Transform playerOverride;
    [Min(1)] [SerializeField] private int maxFramesToFindPlayer = 240;

    private bool playerNearby;
    private bool loading;

    private void Start()
    {
        SetPromptVisible(false);
        StartCoroutine(RestorePlayerIfReturning());
    }

    private void Update()
    {
        if (playerNearby && !loading && Input.GetKeyDown(interactKey))
            OpenFlorilegium();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerNearby = true;
            SetPromptVisible(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            playerNearby = false;
            SetPromptVisible(false);
        }
    }

    private bool IsPlayer(Collider2D collider)
    {
        return collider != null &&
               (collider.CompareTag(playerTag) ||
                collider.transform.root.CompareTag(playerTag));
    }

    private void SetPromptVisible(bool visible)
    {
        float alpha = visible ? 1f : 0f;
        if (promptCanvasGroup != null)
            promptCanvasGroup.alpha = alpha;
        else if (promptGraphic != null)
        {
            Color color = promptGraphic.color;
            color.a = alpha;
            promptGraphic.color = color;
        }
    }

    public void OpenFlorilegium()
    {
        if (loading) return;
        if (string.IsNullOrWhiteSpace(waypointId) ||
            string.IsNullOrWhiteSpace(florilegiumSceneName))
        {
            Debug.LogError("[FlorilegiumWaypoint] Waypoint ID and book scene are required.", this);
            return;
        }
        if (!Application.CanStreamedLevelBeLoaded(florilegiumSceneName))
        {
            Debug.LogError("[FlorilegiumWaypoint] Book scene is not in Build Settings: " +
                florilegiumSceneName, this);
            return;
        }
        if (useCorgiLevelManager && LevelManager.Instance == null)
        {
            Debug.LogError("[FlorilegiumWaypoint] Corgi LevelManager is missing.", this);
            return;
        }

        loading = true;
        SetPromptVisible(false);
        FlorilegiumTravelState.BeginVisit(SceneManager.GetActiveScene().name, waypointId.Trim());
        if (useCorgiLevelManager)
            LevelManager.Instance.GotoLevel(florilegiumSceneName,
                levelManagerFadeOut, levelManagerSave);
        else
            SceneManager.LoadScene(florilegiumSceneName);
    }

    private IEnumerator RestorePlayerIfReturning()
    {
        if (!FlorilegiumTravelState.IsReturnFor(
            SceneManager.GetActiveScene().name, waypointId.Trim()))
            yield break;

        // Wait until Corgi's level-spawning logic has had time to run.
        yield return null;
        yield return new WaitForEndOfFrame();

        for (int frame = 0; frame < maxFramesToFindPlayer; frame++)
        {
            Transform player = playerOverride;
            if (player == null)
            {
                GameObject tagged = null;
                try { tagged = GameObject.FindGameObjectWithTag(playerTag); }
                catch (UnityException) { }
                if (tagged != null) player = tagged.transform;
            }

            if (player != null)
            {
                player.position = returnPosition != null
                    ? returnPosition.position : transform.position;
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null) rb.velocity = Vector2.zero;
                FlorilegiumTravelState.ClearReturn();
                yield break;
            }
            yield return null;
        }

        Debug.LogWarning("[FlorilegiumWaypoint] Return point found, but player tagged '" +
                         playerTag + "' did not appear. Return data was preserved.", this);
    }
}
