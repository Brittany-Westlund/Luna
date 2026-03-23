using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class TeacupReceiver : MonoBehaviour
{
    [Header("Holding")]
    public Transform teacupHoldPoint;
    public bool resetLocalScaleOnReceive = true;
    public Vector3 receivedLocalScale = Vector3.one;

    [Header("Optional Sorting Override")]
    [Tooltip("If true, force all SpriteRenderers on the teacup to these sorting settings when attached.")]
    public bool overrideTeacupSorting = false;
    public string sortingLayerName = "Foreground";
    public int orderInLayer = 200;

    [Header("Optional Position Offset")]
    public Vector3 localPositionOffset = Vector3.zero;

    [Header("Icons")]
    public GameObject happyIcon;
    public GameObject cozyIcon;

    [Header("Dialogue Trigger")]
    public DialogueSystemTrigger dialogueTrigger;

    [Header("Required Flowers (must all be present to trigger dialogue)")]
    public string[] requiredFlowers;

    [Header("Timing")]
    public float holdTeaDuration = 2f;
    public float iconDuration = 15f;

    [Header("Debug")]
    public bool debugLogs = false;

    private GameObject heldTeacup;
    private bool hasRequiredIngredients;
    private TeacupOutcomeResponder outcomeResponder;
    private Coroutine activeRoutine;

    private void Awake()
    {
        if (teacupHoldPoint == null)
            teacupHoldPoint = transform.Find("TeacupHoldPoint");

        if (happyIcon == null)
            happyIcon = transform.Find("HappyIcon")?.gameObject;

        if (cozyIcon == null)
            cozyIcon = transform.Find("CozyIcon")?.gameObject;
    }

    private void Start()
    {
        outcomeResponder = GetComponent<TeacupOutcomeResponder>();

        if (happyIcon != null)
            happyIcon.SetActive(false);

        if (cozyIcon != null)
            cozyIcon.SetActive(false);

        if (debugLogs)
        {
            Debug.Log($"[TeacupReceiver] START on {name}. teacupHoldPoint={(teacupHoldPoint != null ? teacupHoldPoint.name : "NULL")}");
        }
    }

    public bool CanReceiveTeacup()
    {
        return heldTeacup == null;
    }

    public void ReceiveTeacup(GameObject teacup)
    {
        if (debugLogs)
            Debug.Log($"[TeacupReceiver] ReceiveTeacup called on {name} with {(teacup != null ? teacup.name : "NULL")}");

        if (teacup == null)
        {
            Debug.LogWarning($"[TeacupReceiver] {name} received null teacup.");
            return;
        }

        if (!CanReceiveTeacup())
        {
            Debug.LogWarning($"[TeacupReceiver] {name} already has a teacup; ignoring new one.");
            return;
        }

        heldTeacup = teacup;

        Transform parentTarget = teacupHoldPoint != null ? teacupHoldPoint : transform;

        // 🔥 Force the entire parent chain active so the teacup can actually render.
        ForceEnableParentChain(parentTarget);

        heldTeacup.SetActive(true);
        ForceEnableTeacupHierarchy(heldTeacup);

        heldTeacup.transform.SetParent(parentTarget, false);
        heldTeacup.transform.localPosition = localPositionOffset;
        heldTeacup.transform.localRotation = Quaternion.identity;

        if (resetLocalScaleOnReceive)
            heldTeacup.transform.localScale = receivedLocalScale;

        if (overrideTeacupSorting)
            ApplySortingOverrideToTeacup(heldTeacup);

        if (debugLogs)
        {
            Debug.Log($"[TeacupReceiver] {name} parented cup to {parentTarget.name}. localPos={heldTeacup.transform.localPosition}, localScale={heldTeacup.transform.localScale}, worldPos={heldTeacup.transform.position}");
        }

        EvaluateTeaIngredients(heldTeacup);

        if (activeRoutine != null)
            StopCoroutine(activeRoutine);

        activeRoutine = StartCoroutine(HandleTeacupRoutine());
    }

    private void EvaluateTeaIngredients(GameObject teacup)
    {
        TeaEffectManager effect = teacup.GetComponent<TeaEffectManager>();

        if (effect == null)
        {
            Debug.LogWarning($"[TeacupReceiver] {name}: TeaEffectManager not found on teacup; cannot check ingredients.");
            hasRequiredIngredients = false;
            return;
        }

        List<string> ingredients = effect.GetIngredients();
        hasRequiredIngredients = true;

        if (requiredFlowers != null && requiredFlowers.Length > 0)
        {
            foreach (string req in requiredFlowers)
            {
                if (!ingredients.Contains(req))
                {
                    hasRequiredIngredients = false;

                    if (debugLogs)
                        Debug.Log($"[TeacupReceiver] {name} missing required flower: {req}");

                    break;
                }
            }
        }

        if (debugLogs)
            Debug.Log($"[TeacupReceiver] {name} hasRequiredIngredients = {hasRequiredIngredients}");
    }

    private IEnumerator HandleTeacupRoutine()
    {
        yield return new WaitForSeconds(holdTeaDuration);

        if (heldTeacup != null)
        {
            Destroy(heldTeacup);
            heldTeacup = null;
        }

        if (happyIcon != null)
            happyIcon.SetActive(true);

        if (cozyIcon != null)
            cozyIcon.SetActive(hasRequiredIngredients);

        if (hasRequiredIngredients && dialogueTrigger != null)
        {
            dialogueTrigger.enabled = false;
            dialogueTrigger.enabled = true;

            if (debugLogs)
                Debug.Log($"[TeacupReceiver] {name}: DialogueSystemTrigger enabled via tea delivery.");
        }

        if (outcomeResponder != null)
            outcomeResponder.HandleTeaOutcome(hasRequiredIngredients);

        yield return new WaitForSeconds(iconDuration);

        if (happyIcon != null)
            happyIcon.SetActive(false);

        if (cozyIcon != null)
            cozyIcon.SetActive(false);

        activeRoutine = null;
    }

    private void ApplySortingOverrideToTeacup(GameObject teacup)
    {
        SpriteRenderer[] renderers = teacup.GetComponentsInChildren<SpriteRenderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            SpriteRenderer sr = renderers[i];
            if (sr == null)
                continue;

            sr.enabled = true;
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = orderInLayer;

            if (debugLogs)
            {
                Debug.Log($"[TeacupReceiver] Renderer {sr.name}: sprite={(sr.sprite != null ? sr.sprite.name : "NULL")}, layer={sr.sortingLayerName}, order={sr.sortingOrder}, active={sr.gameObject.activeInHierarchy}");
            }
        }
    }

    private void ForceEnableParentChain(Transform target)
    {
        if (target == null)
            return;

        Transform current = target;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
            {
                current.gameObject.SetActive(true);

                if (debugLogs)
                    Debug.Log($"[TeacupReceiver] Force-enabled parent chain object: {current.name}");
            }

            current = current.parent;
        }
    }

    private void ForceEnableTeacupHierarchy(GameObject teacup)
    {
        if (teacup == null)
            return;

        if (!teacup.activeSelf)
            teacup.SetActive(true);

        Transform[] allChildren = teacup.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < allChildren.Length; i++)
        {
            Transform t = allChildren[i];
            if (t != null && !t.gameObject.activeSelf)
            {
                t.gameObject.SetActive(true);

                if (debugLogs)
                    Debug.Log($"[TeacupReceiver] Force-enabled teacup child: {t.name}");
            }
        }

        SpriteRenderer[] renderers = teacup.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].enabled = true;
        }
    }

    public GameObject GetHeldTeacup()
    {
        return heldTeacup;
    }

    public void ClearHeldTeacup()
    {
        heldTeacup = null;
    }
}