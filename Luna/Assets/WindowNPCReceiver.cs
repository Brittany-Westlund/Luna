using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;

public class WindowNPCReceiver : MonoBehaviour
{
    [Header("Window NPC States")]
    public GameObject noSporeObject;
    public GameObject happySporeObject;

    [Header("Prompt")]
    public GameObject promptObject;

    [Header("Completion")]
    public bool disableAfterDelivery = true;

    [Header("Happy SFX")]
    public AudioSource happySFX;

    [Header("Final Quest Trigger (assign the exact DialogueSystemTrigger)")]
    public DialogueSystemTrigger lumiaDialogueTrigger;

    [Header("Quest Completion Visual")]
    public GameObject activateOnQuestComplete;

    private bool playerInside = false;
    private bool isComplete = false;

    // ✅ This is the key gate
    private bool canListenForInput = false;

    private LunaSporeSystem lunaSporeSystem;

    void Start()
    {
        if (happySporeObject) happySporeObject.SetActive(false);
        if (noSporeObject) noSporeObject.SetActive(true);
        if (promptObject) promptObject.SetActive(false);

        if (lumiaDialogueTrigger != null)
            lumiaDialogueTrigger.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isComplete) return;

        playerInside = true;
        lunaSporeSystem = other.GetComponent<LunaSporeSystem>();

        if (promptObject)
            promptObject.SetActive(true);

        // ✅ ONLY allow input if she already HAS a spore on entry
        canListenForInput = lunaSporeSystem != null && lunaSporeSystem.HasSporeAttached;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        canListenForInput = false;
        lunaSporeSystem = null;

        if (promptObject)
            promptObject.SetActive(false);
    }

    void Update()
    {
        if (!playerInside || isComplete || lunaSporeSystem == null) return;

        // ✅ If she entered empty-handed, we WAIT until she has a spore
        if (!canListenForInput && lunaSporeSystem.HasSporeAttached)
        {
            canListenForInput = true;
            return;
        }

        if (!canListenForInput) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            DeliverSpore();
        }
    }

    void DeliverSpore()
    {
        isComplete = true;
        canListenForInput = false;

        lunaSporeSystem.DestroyAttachedSpore();

        if (noSporeObject) noSporeObject.SetActive(false);
        if (happySporeObject) happySporeObject.SetActive(true);

        if (happySFX) happySFX.Play();

        if (promptObject) promptObject.SetActive(false);

        StartCoroutine(HappyThenDisable());
    }

    IEnumerator HappyThenDisable()
    {
        yield return new WaitForSeconds(2.5f);

        if (happySporeObject)
            happySporeObject.SetActive(false);

        if (disableAfterDelivery)
            gameObject.SetActive(false);

        CheckForQuestCompletion();
    }

    void CheckForQuestCompletion()
{
    GameObject[] remainingWindows = GameObject.FindGameObjectsWithTag("WindowNPC");

    if (remainingWindows.Length == 0)
    {
        if (lumiaDialogueTrigger != null)
        {
            lumiaDialogueTrigger.enabled = true;
            lumiaDialogueTrigger.gameObject.SetActive(true);
        }

        // ✅ NEW: Activate completion object (fade-in, fairyfly, etc.)
        if (activateOnQuestComplete != null)
        {
            activateOnQuestComplete.SetActive(true);
        }
    }
}


}
