using System.Collections;
using PixelCrushers.DialogueSystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("Music Fade")]
    public AudioSource oldMusic;  // assign in Inspector
    public AudioSource newMusic;  // assign in Inspector
    public float fadeDuration = 2f;

    [Header("UI")]
    public Text timerDisplay;

    [Header("Rest Reference")]
    public LunaRest lunaRest;

    [Header("SFX")]
    public AudioSource sfxPlayer;          // drag in a reusable AudioSource for one-shots
    public AudioClip restSuccessClip;      // assign your rest success sound here

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
                PlayRestSuccessSound();
                StartCoroutine(FadeOutAndLoadScene(creditsSceneName));
            }
            else if (timer <= 0f)
            {
                StartCoroutine(FadeOutAndLoadScene(mainMenuSceneName));
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

        if (oldMusic != null) StartCoroutine(FadeOutMusic(oldMusic));
        if (newMusic != null) newMusic.Play();

        timer = restWindow;
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

        if (!restCountdownStarted && Vector3.Distance(butterfly.transform.position, luna.position) <= dialogueTriggerDistance)
        {
            restCountdownStarted = true;
        }
    }

    void TriggerDialogue()
    {
        if (dialogueTrigger != null)
        {
            dialogueTrigger.OnUse(); // Make sure DST is set to On Use
            dialogueTriggered = true;
        }
    }

    void UpdateTimerUI()
    {
        if (timerDisplay != null)
        {
            int seconds = Mathf.CeilToInt(Mathf.Max(timer, 0));
            timerDisplay.text = FormatTime(seconds);
        }
    }

    string FormatTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }

    void PlayRestSuccessSound()
    {
        if (sfxPlayer != null && restSuccessClip != null)
        {
            sfxPlayer.PlayOneShot(restSuccessClip);
        }
    }

    IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        if (timerDisplay != null) timerDisplay.enabled = false;

        if (newMusic != null) newMusic.Stop();

        if (oldMusic != null)
        {
            float startVol = oldMusic.volume;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                oldMusic.volume = Mathf.Lerp(startVol, 0f, t);
                yield return null;
            }

            oldMusic.volume = 0f;
            oldMusic.Stop();
        }

        yield return new WaitForSeconds(0.1f);

        SceneManager.LoadScene(sceneName);
    }

    IEnumerator FadeOutMusic(AudioSource source)
    {
        float startVol = source.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            source.volume = Mathf.Lerp(startVol, 0f, t);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
    }
}
