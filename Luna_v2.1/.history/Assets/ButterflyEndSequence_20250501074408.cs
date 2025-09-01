using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PixelCrushers;
using System.Collections;
using PixelCrushers.DialogueSystem;

public class ButterflyEndSequence : MonoBehaviour
{
    [Header("Timing")]
    public float initialWaitTime = 300f; // 5 minutes
    public float restWindow = 30f;

    [Header("Butterfly & Luna")]
    public GameObject butterfly;
    public Transform luna;
    public float followSpeed = 2f;
    public float dialogueTriggerDistance = 2f;

    [Header("Dialogue")]
    public DialogueSystemTrigger dialogueTrigger;

    [Header("Music (AudioSource or BackgroundMusic)")]
    public MonoBehaviour oldMusicComponent;
    public MonoBehaviour newMusicComponent;

    private AudioSource oldMusic;
    private AudioSource newMusic;

    [Header("UI")]
    public Text timerDisplay; // Optional - Drag in a UI Text element to show countdown

    [Header("Rest Reference")]
    public LunaRest lunaRest;

    [Header("Scene Names")]
    public string creditsSceneName = "CreditsScene";
    public string mainMenuSceneName = "MainMenu";

    private float timer;
    private bool sequenceStarted = false;
    private bool restCountdownStarted = false;
    private bool dialogueTriggered = false;

    void Start()
    {
        timer = initialWaitTime;
        oldMusic = GetAudioSourceFromComponent(oldMusicComponent);
        newMusic = GetAudioSourceFromComponent(newMusicComponent);
    }

    void Update()
    {
        if (!sequenceStarted)
        {
            timer -= Time.deltaTime;
            UpdateTimerUI();

            if (timer <= 0f)
            {
                StartSequence();
            }
        }
        else if (restCountdownStarted)
        {
            timer -= Time.deltaTime;
            UpdateTimerUI();

            if (!dialogueTriggered && Vector3.Distance(butterfly.transform.position, luna.position) <= dialogueTriggerDistance)
            {
                TriggerDialogue();
            }

            if (lunaRest != null && lunaRest.isResting)
            {
                SaveSystem.SaveToSlot(1);
                StartCoroutine(SaveAndLoadScene(creditsSceneName));

            }
            else if (timer <= 0f)
            {
                SaveSystem.SaveToSlot(1);
                StartCoroutine(SaveAndLoadScene(mainMenuSceneName));


            }
        }
        else
        {
            FollowLuna();
        }
    }

    void StartSequence()
    {
        sequenceStarted = true;
        Debug.Log("🌙 Sequence started: butterfly follows, music shifts.");

        if (oldMusic != null) oldMusic.Stop();
        if (newMusic != null) newMusic.Play();

        timer = restWindow; // Reset timer for rest window
    }

    void FollowLuna()
    {
        if (butterfly != null && luna != null)
        {
            butterfly.transform.position = Vector3.MoveTowards(
                butterfly.transform.position,
                luna.position,
                followSpeed * Time.deltaTime
            );
        }

        // Start rest countdown once butterfly reaches Luna
        if (!restCountdownStarted && Vector3.Distance(butterfly.transform.position, luna.position) <= dialogueTriggerDistance)
        {
            restCountdownStarted = true;
            Debug.Log("🦋 Butterfly reached Luna. Rest countdown started.");
        }
    }

    void TriggerDialogue()
    {
        if (dialogueTrigger != null && !dialogueTriggered)
        {
            dialogueTrigger.OnUse();  // clean, controlled trigger
            dialogueTriggered = true;
        }
    }

    void UpdateTimerUI()
    {
        if (timerDisplay != null)
        {
            int seconds = Mathf.CeilToInt(timer);
            timerDisplay.text = FormatTime(seconds);
        }
    }

    string FormatTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }

    AudioSource GetAudioSourceFromComponent(MonoBehaviour comp)
    {
        if (comp == null) return null;

        return comp.GetComponent<AudioSource>();
    }

    IEnumerator SaveAndLoadScene(string sceneName)
    {
        SaveSystem.SaveToSlot(1);
        yield return new WaitForSeconds(1f); // wait 1 second to ensure save completes
        SceneManager.LoadScene(sceneName);
    }


}
