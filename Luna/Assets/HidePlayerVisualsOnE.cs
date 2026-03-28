using UnityEngine;

public class HidePlayerVisualsAndAudioOnE : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KeyCode hideKey = KeyCode.E;
    [SerializeField] private float exitGraceTime = 0.1f;

    [Header("Auto-Find Names")]
    [SerializeField] private string teacupHoldPointName = "TeacupHoldPoint";

    private bool playerInRange = false;
    private bool isHidden = false;
    private float exitTimer = 0f;

    private GameObject player;
    private Transform playerRoot;
    private AudioSource[] audioSources;
    private Transform teacupHoldPoint;

    private void Update()
    {
        HandleGracefulExit();

        if (playerInRange && Input.GetKeyDown(hideKey))
        {
            ToggleHide();
        }
    }

    private void HandleGracefulExit()
    {
        if (exitTimer <= 0f)
        {
            return;
        }

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

    private void ToggleHide()
    {
        isHidden = !isHidden;

        ApplyAudioState(isHidden);
        ApplyHeldTeacupVisualState(isHidden);
    }

    private void RestorePlayer()
    {
        isHidden = false;

        ApplyAudioState(false);
        ApplyHeldTeacupVisualState(false);
    }

    private void ApplyAudioState(bool hideAudio)
    {
        if (audioSources == null || audioSources.Length == 0)
        {
            return;
        }

        for (int i = 0; i < audioSources.Length; i++)
        {
            AudioSource a = audioSources[i];

            if (a == null)
            {
                continue;
            }

            if (hideAudio)
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

    private void ApplyHeldTeacupVisualState(bool hideCup)
    {
        if (teacupHoldPoint == null)
        {
            return;
        }

        SpriteRenderer[] cupRenderers = teacupHoldPoint.GetComponentsInChildren<SpriteRenderer>(true);

        if (cupRenderers == null || cupRenderers.Length == 0)
        {
            return;
        }

        for (int i = 0; i < cupRenderers.Length; i++)
        {
            SpriteRenderer cupRenderer = cupRenderers[i];

            if (cupRenderer == null)
            {
                continue;
            }

            cupRenderer.enabled = !hideCup;
        }
    }

    private void CachePlayerReferences(GameObject targetPlayer)
    {
        if (targetPlayer == null)
        {
            return;
        }

        player = targetPlayer;
        playerRoot = targetPlayer.transform.root;
        audioSources = player.GetComponentsInChildren<AudioSource>(true);
        teacupHoldPoint = FindChildRecursive(playerRoot, teacupHoldPointName);
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        if (parent.name == childName)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            Transform result = FindChildRecursive(child, childName);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = true;
        exitTimer = 0f;

        if (player == null || player != other.gameObject)
        {
            CachePlayerReferences(other.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = true;
        exitTimer = 0f;

        if (player == null || player != other.gameObject)
        {
            CachePlayerReferences(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        exitTimer = exitGraceTime;
    }
}