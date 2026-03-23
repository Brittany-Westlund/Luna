using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class GentleRustleWithAudio : MonoBehaviour
{
    public enum StopMode
    {
        None,               // Once started, keeps going forever
        StopOnExit,         // Stops when player exits, can start again later
        StopUntilNextEnter, // Same practical behavior as StopOnExit here; restarts on re-entry
        StopForever         // Stops on exit and never rustles again
    }

    [Header("Motion")]
    [SerializeField] private float positionAmount = 0.03f;
    [SerializeField] private float rotationAmount = 2f;
    [SerializeField] private float speed = 2f;

    [Header("Smoothing")]
    [SerializeField] private float fadeInDuration = 0.15f;
    [SerializeField] private float fadeOutDuration = 0.25f;

    [Header("Trigger Behavior")]
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private bool useTrigger = true;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private StopMode stopMode = StopMode.StopOnExit;

    [Header("Audio (optional)")]
    [SerializeField] private AudioSource audioSourceA;
    [SerializeField] private AudioSource audioSourceB;
    [SerializeField] private float maxAudioVolume = 1f;
    [SerializeField] private bool disableAudioSourcesWhenStopped = true;

    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;
    private float timeOffset;

    private float rustleStrength = 0f;
    private float targetStrength = 0f;

    private bool playerInside = false;
    private bool hasStoppedForever = false;
    private bool wasRustlingLastFrame = false;

    private void Awake()
    {
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;
        timeOffset = Random.Range(0f, 100f);

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }

        PrepareAudioSource(audioSourceA);
        PrepareAudioSource(audioSourceB);
    }

    private void Start()
    {
        if (playOnStart)
        {
            StartRustle();
        }
    }

    private void Update()
    {
        float duration = targetStrength > rustleStrength ? fadeInDuration : fadeOutDuration;
        duration = Mathf.Max(0.0001f, duration);

        rustleStrength = Mathf.MoveTowards(rustleStrength, targetStrength, Time.deltaTime / duration);

        bool currentlyRustling = rustleStrength > 0.0001f;

        if (currentlyRustling)
        {
            float t = Time.time * speed + timeOffset;

            float x = Mathf.Sin(t) * positionAmount * rustleStrength;
            float y = Mathf.Cos(t * 0.8f) * positionAmount * rustleStrength;
            transform.localPosition = startLocalPosition + new Vector3(x, y, 0f);

            float zRot = Mathf.Sin(t * 1.2f) * rotationAmount * rustleStrength;
            transform.localRotation = startLocalRotation * Quaternion.Euler(0f, 0f, zRot);
        }
        else
        {
            transform.localPosition = startLocalPosition;
            transform.localRotation = startLocalRotation;
        }

        UpdateAudio(currentlyRustling);

        wasRustlingLastFrame = currentlyRustling;
    }

    public void StartRustle()
    {
        if (hasStoppedForever) return;

        targetStrength = 1f;
    }

    public void StopRustle()
    {
        targetStrength = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!useTrigger) return;
        if (!other.CompareTag(playerTag)) return;
        if (hasStoppedForever) return;

        playerInside = true;
        StartRustle();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!useTrigger) return;
        if (!other.CompareTag(playerTag)) return;

        playerInside = false;

        if (stopMode == StopMode.StopOnExit || stopMode == StopMode.StopUntilNextEnter)
        {
            StopRustle();
        }
        else if (stopMode == StopMode.StopForever)
        {
            hasStoppedForever = true;
            StopRustle();
            enabled = false;

            // Force final reset immediately since script is disabling
            transform.localPosition = startLocalPosition;
            transform.localRotation = startLocalRotation;

            StopAndDisableAudio(audioSourceA);
            StopAndDisableAudio(audioSourceB);
        }
    }

    private void UpdateAudio(bool currentlyRustling)
    {
        float targetVolume = currentlyRustling ? maxAudioVolume * rustleStrength : 0f;

        UpdateSingleAudio(audioSourceA, targetVolume, currentlyRustling);
        UpdateSingleAudio(audioSourceB, targetVolume, currentlyRustling);

        if (!currentlyRustling && wasRustlingLastFrame)
        {
            if (disableAudioSourcesWhenStopped)
            {
                StopAndDisableAudio(audioSourceA);
                StopAndDisableAudio(audioSourceB);
            }
            else
            {
                StopAudio(audioSourceA);
                StopAudio(audioSourceB);
            }
        }
    }

    private void UpdateSingleAudio(AudioSource source, float targetVolume, bool currentlyRustling)
    {
        if (source == null) return;

        if (currentlyRustling)
        {
            if (disableAudioSourcesWhenStopped && !source.enabled)
            {
                source.enabled = true;
            }

            if (!source.isPlaying)
            {
                source.Play();
            }

            source.volume = targetVolume;
        }
    }

    private void PrepareAudioSource(AudioSource source)
    {
        if (source == null) return;

        source.playOnAwake = false;
        source.loop = true;
        source.volume = 0f;

        if (disableAudioSourcesWhenStopped)
        {
            source.enabled = false;
        }
    }

    private void StopAudio(AudioSource source)
    {
        if (source == null) return;

        if (source.isPlaying)
        {
            source.Stop();
        }

        source.volume = 0f;
    }

    private void StopAndDisableAudio(AudioSource source)
    {
        if (source == null) return;

        if (source.isPlaying)
        {
            source.Stop();
        }

        source.volume = 0f;
        source.enabled = false;
    }
}