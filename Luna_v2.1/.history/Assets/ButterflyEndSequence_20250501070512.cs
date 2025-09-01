using UnityEngine;
using UnityEngine.SceneManagement;

public class ButterflyEndSequence : MonoBehaviour
{
    [Header("Timing")]
    public float timeToStartSequence = 300f; // 5 min
    public float restWindow = 30f;

    [Header("Butterfly & Luna")]
    public GameObject butterfly;
    public Transform luna;
    public float followSpeed = 2f;
    public float dialogueTriggerDistance = 2f;

    [Header("Music")]
    public AudioSource oldMusic;
    public AudioSource newMusic;

    [Header("Dialogue Trigger")]
    public GameObject dialogueTriggerObject;

    [Header("Rest Detection")]
    public LunaRest lunaRest; // a script that tracks if Luna is resting

    [Header("Scene Names")]
    public string creditsSceneName = "CreditsScene";
    public string mainMenuSceneName = "MainMenu";
    public bool isResting = false;
    private float timer;
    private bool sequenceStarted = false;
    private bool dialogueTriggered = false;
    private bool restCountdownStarted = false;

    void Start()
    {
        timer = timeToStartSequence;
    }

    void Update()
    {
        if (!sequenceStarted)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                StartSequence();
            }
        }
        else if (restCountdownStarted)
        {
            timer -= Time.deltaTime;

            if (!dialogueTriggered && Vector3.Distance(butterfly.transform.position, luna.position) <= dialogueTriggerDistance)
            {
                TriggerDialogue();
            }

            if (lunaRest.isResting)

            {
                SceneManager.LoadScene(creditsSceneName);
            }
            else if (timer <= 0)
            {
                SceneManager.LoadScene(mainMenuSceneName);
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

        // Stop old music and start new
        if (oldMusic != null) oldMusic.Stop();
        if (newMusic != null) newMusic.Play();

        // Begin butterfly follow
        Debug.Log("Butterfly now follows Luna.");
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

        if (Vector3.Distance(butterfly.transform.position, luna.position) <= dialogueTriggerDistance)
        {
            StartRestCountdown();
        }
    }

    void StartRestCountdown()
    {
        restCountdownStarted = true;
        timer = restWindow;
    }

    void TriggerDialogue()
    {
        if (dialogueTriggerObject != null)
        {
            dialogueTriggerObject.SetActive(true); // Assumes dialogue autoplays on enable
            dialogueTriggered = true;
        }
    }
}
