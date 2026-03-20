using UnityEngine;

public class HidePlayerVisualsAndAudioOnE : MonoBehaviour
{
    private bool playerInRange = false;
    private bool isHidden = false;

    private float exitTimer = 0f;
    private float exitGraceTime = 0.1f;

    private GameObject player;
    private SpriteRenderer sr;
    private Animator anim;
    private AudioSource[] audioSources;

    private void Update()
    {
        // Handle graceful trigger exit
        if (exitTimer > 0f)
        {
            exitTimer -= Time.deltaTime;

            if (exitTimer <= 0f)
            {
                playerInRange = false;

                if (isHidden)
                {
                    RestorePlayer();
                }
            }
        }

        // Interaction
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            ToggleHide();
        }
    }

    private void ToggleHide()
    {
        isHidden = !isHidden;

        if (sr != null) sr.enabled = !isHidden;
        if (anim != null) anim.enabled = !isHidden;

        if (audioSources != null)
        {
            foreach (var a in audioSources)
            {
                if (isHidden)
                {
                    a.Pause();
                    a.mute = true;
                }
                else
                {
                    a.mute = false;
                    a.UnPause();
                }
            }
        }
    }

    private void RestorePlayer()
    {
        isHidden = false;

        if (sr != null) sr.enabled = true;
        if (anim != null) anim.enabled = true;

        if (audioSources != null)
        {
            foreach (var a in audioSources)
            {
                a.mute = false;
                a.UnPause();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        exitTimer = 0f;

        if (player == null)
        {
            player = other.gameObject;

            sr = player.GetComponentInChildren<SpriteRenderer>(true);
            anim = player.GetComponentInChildren<Animator>(true);
            audioSources = player.GetComponentsInChildren<AudioSource>(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        exitTimer = exitGraceTime;
    }
}