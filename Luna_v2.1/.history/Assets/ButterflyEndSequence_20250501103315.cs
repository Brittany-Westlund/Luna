using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PixelCrushers.DialogueSystem;
using System.Collections;

public class ButterflyEndSequence : MonoBehaviour
{
    [Header("Timing")]
    public float initialWaitTime = 300f;
    public float restWindow = 30f;

    [Header("Butterfly & Luna")]
    public GameObject butterfly;
    public Transform luna;
    public float followSpeed = 2f;
    public float dialogueTriggerDistance = 2f;

    [Header("Dialogue")]
    public DialogueSystemTrigger dialogueTrigger;

    [Header("Music Fade")]
    public AudioSource oldMusic;
    public AudioSource newMusic;
    public float musicFadeDuration = 1f;

    [Header("UI")]
    public Text timerDisplay;
    public GameObject restHintIcon;

    [Header("Rest Reference")]
    public LunaRest lunaRest;

    [Header("SFX")]
    public AudioSource sfxPlayer;
    public AudioClip restSuccessClip;
    [Range(0f, 1f)] public float restSuccessVolume = 0.5f;

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
        
        if (newMusic != null) newMusic.enabled = false;
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
                StartCoroutine(FadeOutAndLoadScene(creditsSceneName, 2f));
            }
            else if (timer <= 0f)
            {
                StartCoroutine(FadeOutAndLoadScene(mainMenuSceneName));
            }
        }
        else
        {
            FollowLuna();

            if (!restCountdownStarted && Vector3.Distance(butterfly.transform.position, luna.position) <= dialogueTriggerDistance)
            {
                restCountdownStarted = true;
            }
        }
        //if (Input.GetKeyDown(KeyCode.Z))
        //{
         //   restHintIcon.SetActive(false);
        //}
    }

    void StartSequence()
    {
        sequenceStarted = true;
        timer = restWindow;

        restHintIcon.SetActive(true);
        restHintIcon.transform.parent.gameObject.SetActive(true); // Luna
        restHintIcon.transform.parent.parent.gameObject.SetActive(true); // ScaledLuna

        if (oldMusic != null)
            StartCoroutine(FadeOutOldMusicThenStartNew());
        else if (newMusic != null)
        {
            newMusic.enabled = true;
            newMusic.Play();
        }
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
    }

    void TriggerDialogue()
    {
        if (dialogueTrigger != null)
        {
            dialogueTrigger.OnUse();
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
            sfxPlayer.PlayOneShot(restSuccessClip, restSuccessVolume);
        }
    }

    IEnumerator FadeOutOldMusicThenStartNew()
    {
        if (oldMusic != null && oldMusic.isPlaying)
        {
            float startVolume = oldMusic.volume;
            float elapsed = 0f;

            while (elapsed < musicFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / musicFadeDuration;
                oldMusic.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            oldMusic.volume = 0f;
            oldMusic.Stop();
        }

        if (newMusic != null)
        {
            newMusic.enabled = true;
            newMusic.Play();
        }
    }

    IEnumerator FadeOutAndLoadScene(string sceneName, float delayBeforeLoad = 0f)
    {
        if (timerDisplay != null)
            timerDisplay.enabled = false;

        if (delayBeforeLoad > 0f)
            yield return new WaitForSeconds(delayBeforeLoad);

        SceneManager.LoadScene(sceneName);
    }
}
 