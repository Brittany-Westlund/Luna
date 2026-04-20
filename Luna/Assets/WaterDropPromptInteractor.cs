using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WaterDropPromptInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CustomInteractionFeedback customInteractionFeedback;
    [SerializeField] private SpriteRenderer mainWaterSpriteRenderer;

    [Header("Search")]
    [SerializeField] private string teapotTag = "Teapot";
    [SerializeField] private float teapotSearchRadius = 2f;

    [Header("Auto Interaction")]
    [SerializeField] private float autoInteractDelay = 1.5f;

    [Header("Respawn")]
    [SerializeField] private float cooldownDuration = 5f;
    [SerializeField] private float fadeOutDuration = 0.15f;
    [SerializeField] private float scaleOutDuration = 0.15f;
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float scaleInDuration = 0.75f;

    [Header("Behavior")]
    [SerializeField] private bool hidePromptOnExit = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogging = false;

    private Vector3 initialScale;
    private float initialAlpha = 1f;

    private bool playerOnLilypad;
    private bool isAnimating;
    private bool isOnCooldown;
    private bool promptShowing;

    private float autoTimer = 0f;

    private TeapotWaterReceiver currentReceiver;
    private Coroutine respawnRoutine;

    private void Awake()
    {
        initialScale = transform.localScale;

        if (mainWaterSpriteRenderer != null)
        {
            initialAlpha = mainWaterSpriteRenderer.color.a;
            if (Mathf.Approximately(initialAlpha, 0f))
            {
                initialAlpha = 1f;
            }

            mainWaterSpriteRenderer.enabled = true;
            SetMainAlpha(1f);
        }

        transform.localScale = initialScale;
    }

    private void OnEnable()
    {
        if (customInteractionFeedback != null)
        {
            customInteractionFeedback.gameObject.SetActive(false);
        }

        promptShowing = false;
        autoTimer = 0f;
        playerOnLilypad = false;
        currentReceiver = null;
    }

    private void Update()
    {
        // If player is standing on lilypad, keep trying to find a valid teapot.
        // This is what lets the water drop begin after the teapot spawns,
        // without requiring the player to leave and re-enter.
        if (playerOnLilypad && !promptShowing && !isAnimating && !isOnCooldown)
        {
            TryShowPrompt();
        }

        if (!promptShowing || isAnimating || isOnCooldown || !playerOnLilypad)
        {
            autoTimer = 0f;
            return;
        }

        autoTimer += Time.deltaTime;

        if (autoTimer >= autoInteractDelay)
        {
            autoTimer = 0f;
            HandleInteraction();
        }
    }

    public void SetPlayerOnLilypad(bool value)
    {
        playerOnLilypad = value;

        if (debugLogging)
        {
            Debug.Log($"[WaterDropPromptInteractor] SetPlayerOnLilypad({value}) on '{name}'.");
        }

        if (!playerOnLilypad)
        {
            currentReceiver = null;
            autoTimer = 0f;

            if (hidePromptOnExit)
            {
                HidePrompt();
            }

            return;
        }

        TryShowPrompt();
    }

    private void TryShowPrompt()
    {
        if (!playerOnLilypad)
            return;

        if (isAnimating || isOnCooldown)
            return;

        if (!HasVisibleWater())
            return;

        currentReceiver = FindNearestTeapotReceiverNeedingWater();

        if (currentReceiver == null)
        {
            HidePrompt();

            if (debugLogging)
            {
                Debug.Log("[WaterDropPromptInteractor] No nearby teapot needing water yet.");
            }

            return;
        }

        ShowPrompt();
    }

    private void ShowPrompt()
    {
        if (customInteractionFeedback == null)
            return;

        if (!customInteractionFeedback.gameObject.activeSelf)
        {
            customInteractionFeedback.gameObject.SetActive(true);
        }

        promptShowing = true;
        autoTimer = 0f;

        if (debugLogging)
        {
            Debug.Log("[WaterDropPromptInteractor] Prompt shown.");
        }
    }

    private void HidePrompt()
    {
        if (customInteractionFeedback != null && customInteractionFeedback.gameObject.activeSelf)
        {
            customInteractionFeedback.gameObject.SetActive(false);
        }

        promptShowing = false;
        autoTimer = 0f;

        if (debugLogging)
        {
            Debug.Log("[WaterDropPromptInteractor] Prompt hidden.");
        }
    }

    private TeapotWaterReceiver FindNearestTeapotReceiverNeedingWater()
    {
        GameObject[] teapots = GameObject.FindGameObjectsWithTag(teapotTag);

        TeapotWaterReceiver closest = null;
        float closestDist = float.MaxValue;
        float maxDistSq = teapotSearchRadius * teapotSearchRadius;

        for (int i = 0; i < teapots.Length; i++)
        {
            GameObject teapot = teapots[i];
            if (teapot == null)
                continue;

            float d = ((Vector2)(teapot.transform.position - transform.position)).sqrMagnitude;
            if (d > maxDistSq)
                continue;

            TeapotWaterReceiver receiver = teapot.GetComponent<TeapotWaterReceiver>();
            if (receiver == null)
            {
                receiver = teapot.GetComponentInChildren<TeapotWaterReceiver>(true);
            }

            if (receiver == null)
                continue;

            if (receiver.HasWater())
                continue;

            if (d < closestDist)
            {
                closestDist = d;
                closest = receiver;
            }
        }

        return closest;
    }

    private void HandleInteraction()
    {
        if (debugLogging)
        {
            Debug.Log("[WaterDropPromptInteractor] Interaction triggered.");
        }

        promptShowing = false;
        autoTimer = 0f;

        if (!playerOnLilypad)
            return;

        if (isAnimating || isOnCooldown)
            return;

        if (currentReceiver == null)
        {
            currentReceiver = FindNearestTeapotReceiverNeedingWater();
        }

        if (currentReceiver == null)
        {
            if (debugLogging)
            {
                Debug.LogWarning("[WaterDropPromptInteractor] No teapot receiver found on interaction.");
            }
            return;
        }

        currentReceiver.ReceiveWater();

        if (respawnRoutine != null)
        {
            StopCoroutine(respawnRoutine);
        }

        respawnRoutine = StartCoroutine(ConsumeAndRespawnRoutine());
    }

    private IEnumerator ConsumeAndRespawnRoutine()
    {
        isAnimating = true;
        isOnCooldown = true;

        HidePrompt();

        float elapsed = 0f;
        float outTime = Mathf.Max(fadeOutDuration, scaleOutDuration);
        Vector3 startScale = transform.localScale;

        while (elapsed < outTime)
        {
            elapsed += Time.deltaTime;

            float alphaT = fadeOutDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / fadeOutDuration);
            float scaleT = scaleOutDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / scaleOutDuration);

            SetMainAlpha(Mathf.Lerp(1f, 0f, alphaT));
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, scaleT);

            yield return null;
        }

        SetMainAlpha(0f);
        transform.localScale = Vector3.zero;

        isAnimating = false;

        if (debugLogging)
        {
            Debug.Log($"[WaterDropPromptInteractor] Cooldown started for {cooldownDuration} seconds.");
        }

        yield return new WaitForSeconds(cooldownDuration);

        isAnimating = true;

        elapsed = 0f;
        float inTime = Mathf.Max(fadeInDuration, scaleInDuration);

        while (elapsed < inTime)
        {
            elapsed += Time.deltaTime;

            float alphaT = fadeInDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / fadeInDuration);
            float scaleT = scaleInDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / scaleInDuration);

            SetMainAlpha(Mathf.Lerp(0f, 1f, alphaT));
            transform.localScale = Vector3.Lerp(Vector3.zero, initialScale, scaleT);

            yield return null;
        }

        SetMainAlpha(1f);
        transform.localScale = initialScale;

        isAnimating = false;
        isOnCooldown = false;

        if (debugLogging)
        {
            Debug.Log("[WaterDropPromptInteractor] WaterDrop recharged.");
        }

        if (playerOnLilypad)
        {
            TryShowPrompt();
        }
    }

    private bool HasVisibleWater()
    {
        if (mainWaterSpriteRenderer == null)
            return false;

        return mainWaterSpriteRenderer.enabled && mainWaterSpriteRenderer.color.a > 0.01f;
    }

    private void SetMainAlpha(float normalized)
    {
        if (mainWaterSpriteRenderer == null)
            return;

        mainWaterSpriteRenderer.enabled = true;

        Color c = mainWaterSpriteRenderer.color;
        c.a = initialAlpha * Mathf.Clamp01(normalized);
        mainWaterSpriteRenderer.color = c;
    }
}