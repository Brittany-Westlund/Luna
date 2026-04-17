using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class AcornReceiver : MonoBehaviour
{
    [Header("Input")]
    public KeyCode interactKey = KeyCode.E;

    [Header("Detection")]
    public string playerTag = "Player";
    public string acornTag = "Acorn";

    [Header("Held Object Search")]
    [Tooltip("Optional explicit hold point to search for the held acorn. If left blank, the script will search the player hierarchy.")]
    public Transform playerHoldPoint;

    [Tooltip("If true, search the entire player hierarchy for an active acorn if no hold point is assigned or no acorn is found there.")]
    public bool searchEntirePlayerHierarchy = true;

    [Header("NPC Acorn Visuals")]
    [Tooltip("Optional NPC child object that shows the whole acorn first.")]
    public GameObject acornWholeObject;

    [Tooltip("Optional NPC child object that shows the lantern acorn after the delay.")]
    public GameObject acornLanternObject;

    [Tooltip("If true, show the whole acorn first, then switch to lantern after the delay.")]
    public bool showWholeAcornBeforeLantern = true;

    [Header("Timing")]
    public float switchDelay = 2f;

    [Header("Audio")]
    [Tooltip("AudioSource used for the acorn-holding/working sound.")]
    public AudioSource acornWholeAudioSource;

    [Tooltip("Sound to play while the squirrel is holding/working on the whole acorn.")]
    public AudioClip acornWholeSFX;

    [Tooltip("If true, the whole-acorn sound loops until the lantern becomes available.")]
    public bool loopAcornWholeSFX = true;

    [Tooltip("Sound to play when the lantern acorn becomes available.")]
    public AudioClip acornLanternReadySFX;

    [Tooltip("Optional separate AudioSource for the lantern-ready sound. If left null, uses acornWholeAudioSource.")]
    public AudioSource acornLanternReadyAudioSource;

    [Header("Events")]
    [Tooltip("Fires immediately after the player's held acorn is received and disabled.")]
    public UnityEvent onAcornReceived;

    [Tooltip("Fires after switchDelay. Use this for any extra reactions when lantern state begins.")]
    public UnityEvent onSwitchToLantern;

    [Header("Options")]
    [Tooltip("If true, this receiver can only be used once.")]
    public bool receiveOnlyOnce = true;

    [Tooltip("If true, the player's acorn GameObject is set inactive instead of destroyed.")]
    public bool disableReceivedAcorn = true;

    [Tooltip("If true, the player's acorn GameObject is destroyed instead of disabled.")]
    public bool destroyReceivedAcorn = false;

    [Header("Debug")]
    public bool debugLogs = false;

    private GameObject playerInRange;
    private bool hasReceived = false;
    private bool isProcessing = false;
    private Coroutine receiveRoutine;

    private void Awake()
    {
        InitializeNpcVisualState();

        if (acornLanternReadyAudioSource == null)
            acornLanternReadyAudioSource = acornWholeAudioSource;
    }

    private void Start()
    {
        InitializeNpcVisualState();

        if (acornLanternReadyAudioSource == null)
            acornLanternReadyAudioSource = acornWholeAudioSource;
    }

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    private void InitializeNpcVisualState()
    {
        if (acornWholeObject != null)
            acornWholeObject.SetActive(false);

        if (acornLanternObject != null)
            acornLanternObject.SetActive(false);

        StopWholeAcornSFX();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInRange = other.gameObject;

        if (debugLogs)
            Debug.Log($"[AcornReceiver] {name}: Player entered range.");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (playerInRange == null)
            return;

        if (other.gameObject == playerInRange)
        {
            if (debugLogs)
                Debug.Log($"[AcornReceiver] {name}: Player exited range.");

            playerInRange = null;
        }
    }

    private void Update()
    {
        if (playerInRange == null)
            return;

        if (isProcessing)
            return;

        if (receiveOnlyOnce && hasReceived)
            return;

        if (Input.GetKeyDown(interactKey))
        {
            TryReceiveAcorn();
        }
    }

    public void TryReceiveAcorn()
    {
        if (playerInRange == null)
        {
            if (debugLogs)
                Debug.Log($"[AcornReceiver] {name}: No player in range.");
            return;
        }

        GameObject heldAcorn = FindHeldAcorn(playerInRange);

        if (heldAcorn == null)
        {
            if (debugLogs)
                Debug.Log($"[AcornReceiver] {name}: No held acorn found on player.");
            return;
        }

        if (debugLogs)
            Debug.Log($"[AcornReceiver] {name}: Receiving acorn {heldAcorn.name}");

        if (receiveRoutine != null)
            StopCoroutine(receiveRoutine);

        receiveRoutine = StartCoroutine(ReceiveAcornRoutine(heldAcorn));
    }

    private IEnumerator ReceiveAcornRoutine(GameObject heldAcorn)
    {
        isProcessing = true;
        hasReceived = true;

        if (destroyReceivedAcorn)
        {
            Destroy(heldAcorn);

            if (debugLogs)
                Debug.Log($"[AcornReceiver] {name}: Destroyed received acorn.");
        }
        else if (disableReceivedAcorn)
        {
            heldAcorn.SetActive(false);

            if (debugLogs)
                Debug.Log($"[AcornReceiver] {name}: Disabled received acorn.");
        }

        if (showWholeAcornBeforeLantern)
        {
            if (acornWholeObject != null)
                acornWholeObject.SetActive(true);

            if (acornLanternObject != null)
                acornLanternObject.SetActive(false);

            PlayWholeAcornSFX();

            if (debugLogs)
                Debug.Log($"[AcornReceiver] {name}: Showing whole acorn.");
        }
        else
        {
            if (acornWholeObject != null)
                acornWholeObject.SetActive(false);

            if (acornLanternObject != null)
                acornLanternObject.SetActive(true);

            StopWholeAcornSFX();
            PlayLanternReadySFX();

            if (debugLogs)
                Debug.Log($"[AcornReceiver] {name}: Skipping whole acorn and showing lantern immediately.");
        }

        onAcornReceived?.Invoke();

        if (debugLogs)
            Debug.Log($"[AcornReceiver] {name}: Fired onAcornReceived.");

        if (showWholeAcornBeforeLantern && switchDelay > 0f)
            yield return new WaitForSeconds(switchDelay);

        if (showWholeAcornBeforeLantern)
        {
            if (acornWholeObject != null)
                acornWholeObject.SetActive(false);

            if (acornLanternObject != null)
                acornLanternObject.SetActive(true);

            StopWholeAcornSFX();
            PlayLanternReadySFX();

            if (debugLogs)
                Debug.Log($"[AcornReceiver] {name}: Switched from whole acorn to lantern.");
        }

        onSwitchToLantern?.Invoke();

        if (debugLogs)
            Debug.Log($"[AcornReceiver] {name}: Fired onSwitchToLantern.");

        isProcessing = false;
        receiveRoutine = null;
    }

    private void PlayWholeAcornSFX()
    {
        if (acornWholeAudioSource == null || acornWholeSFX == null)
            return;

        acornWholeAudioSource.clip = acornWholeSFX;
        acornWholeAudioSource.loop = loopAcornWholeSFX;
        acornWholeAudioSource.Play();

        if (debugLogs)
            Debug.Log($"[AcornReceiver] {name}: Playing whole acorn SFX.");
    }

    private void StopWholeAcornSFX()
    {
        if (acornWholeAudioSource == null)
            return;

        if (acornWholeAudioSource.isPlaying)
            acornWholeAudioSource.Stop();
    }

    private void PlayLanternReadySFX()
    {
        if (acornLanternReadyAudioSource == null || acornLanternReadySFX == null)
            return;

        acornLanternReadyAudioSource.PlayOneShot(acornLanternReadySFX);

        if (debugLogs)
            Debug.Log($"[AcornReceiver] {name}: Playing lantern ready SFX.");
    }

    private GameObject FindHeldAcorn(GameObject player)
    {
        if (player == null)
            return null;

        if (playerHoldPoint != null)
        {
            GameObject fromHoldPoint = FindTaggedObjectInHierarchy(playerHoldPoint, acornTag, activeOnly: true);
            if (fromHoldPoint != null)
                return fromHoldPoint;
        }

        if (searchEntirePlayerHierarchy)
        {
            GameObject fromPlayer = FindTaggedObjectInHierarchy(player.transform, acornTag, activeOnly: true);
            if (fromPlayer != null)
                return fromPlayer;
        }

        return null;
    }

    private GameObject FindTaggedObjectInHierarchy(Transform root, string requiredTag, bool activeOnly)
    {
        if (root == null)
            return null;

        Transform[] allChildren = root.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < allChildren.Length; i++)
        {
            Transform t = allChildren[i];
            if (t == null)
                continue;

            GameObject obj = t.gameObject;
            if (obj == null)
                continue;

            if (activeOnly && !obj.activeInHierarchy)
                continue;

            if (obj.CompareTag(requiredTag))
                return obj;
        }

        return null;
    }

    public bool HasReceived()
    {
        return hasReceived;
    }

    public void ResetReceiver()
    {
        if (receiveRoutine != null)
        {
            StopCoroutine(receiveRoutine);
            receiveRoutine = null;
        }

        hasReceived = false;
        isProcessing = false;

        InitializeNpcVisualState();

        if (debugLogs)
            Debug.Log($"[AcornReceiver] {name}: Receiver reset.");
    }
}