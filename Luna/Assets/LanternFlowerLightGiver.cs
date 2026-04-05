using UnityEngine;
using System.Linq;
using System.Collections;

public class LanternFlowerLightGiver : MonoBehaviour
{
    [Header("Input")]
    public KeyCode giveKey = KeyCode.E;

    [Header("Search")]
    public float lightRadius = 1.2f;

    [Header("Lantern")]
    public LanternSmartToggle lanternToggle;

    [Header("Auto Give")]
    public bool autoGiveLight = false;
    public float autoGiveDelay = 0.25f;

    [Header("Prompt")]
    public string lightPromptObjectName = "LightPrompt";

    [Header("Debug")]
    public bool debugLogs = false;

    private bool isAutoGiving = false;

    private void Awake()
    {
        if (lanternToggle == null)
            lanternToggle = GetComponent<LanternSmartToggle>();
    }

    private void Update()
    {
        if (lanternToggle == null)
            return;

        if (!lanternToggle.IsLit)
            return;

        if (autoGiveLight)
        {
            if (!isAutoGiving)
            {
                SproutAndLightManager flower = FindValidFlower();
                if (flower != null)
                {
                    StartCoroutine(AutoGiveRoutine(flower));
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(giveKey))
            {
                TryGiveLightToFlower();
            }
        }
    }

    private IEnumerator AutoGiveRoutine(SproutAndLightManager flower)
    {
        isAutoGiving = true;

        yield return new WaitForSeconds(autoGiveDelay);

        if (flower != null && CanLightFlower(flower) && lanternToggle != null && lanternToggle.IsLit)
        {
            if (debugLogs)
                Debug.Log($"[LanternFlowerLightGiver] AUTO lighting flower: {flower.name}");

            LightFlower(flower);
            lanternToggle.ExtinguishLantern();
        }

        isAutoGiving = false;
    }

    private void TryGiveLightToFlower()
    {
        SproutAndLightManager flower = FindValidFlower();

        if (flower == null)
        {
            if (debugLogs)
                Debug.Log("[LanternFlowerLightGiver] No valid flower in range.");
            return;
        }

        if (debugLogs)
            Debug.Log($"[LanternFlowerLightGiver] MANUAL lighting flower: {flower.name}");

        LightFlower(flower);
        lanternToggle.ExtinguishLantern();
    }

    private SproutAndLightManager FindValidFlower()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, lightRadius);

        return hits
            .Select(h =>
                h.GetComponent<SproutAndLightManager>()
                ?? h.GetComponentInChildren<SproutAndLightManager>()
                ?? h.GetComponentInParent<SproutAndLightManager>())
            .FirstOrDefault(f => f != null && CanLightFlower(f));
    }

    private bool CanLightFlower(SproutAndLightManager flower)
    {
        if (flower == null)
            return false;

        if (!flower.IsFullyGrown)
        {
            if (debugLogs)
                Debug.Log($"[LanternFlowerLightGiver] {flower.name} is not fully grown.");
            return false;
        }

        if (IsFlowerAlreadyLit(flower))
        {
            if (debugLogs)
                Debug.Log($"[LanternFlowerLightGiver] {flower.name} is already lit.");
            return false;
        }

        return true;
    }

    private bool IsFlowerAlreadyLit(SproutAndLightManager flower)
    {
        if (flower == null)
            return false;

        if (flower.litFlowerRenderer != null && flower.litFlowerRenderer.enabled)
            return true;

        Transform litChild = FindDeepChildByName(flower.transform, "LitFlowerB");
        if (litChild != null)
        {
            SpriteRenderer sr = litChild.GetComponent<SpriteRenderer>();
            if (sr != null && sr.enabled)
                return true;

            if (litChild.gameObject.activeSelf && sr == null)
                return true;
        }

        return false;
    }

    private void LightFlower(SproutAndLightManager flower)
    {
        if (flower == null)
            return;

        if (flower.litFlowerRenderer != null)
        {
            flower.litFlowerRenderer.enabled = true;

            Color c = flower.litFlowerRenderer.color;
            c.a = 1f;
            flower.litFlowerRenderer.color = c;

            if (debugLogs)
                Debug.Log($"[LanternFlowerLightGiver] Enabled litFlowerRenderer on {flower.name}");
        }

        Transform litChild = FindDeepChildByName(flower.transform, "LitFlowerB");
        if (litChild != null)
        {
            litChild.gameObject.SetActive(true);

            SpriteRenderer sr = litChild.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = true;

                Color c = sr.color;
                c.a = 1f;
                sr.color = c;
            }

            if (debugLogs)
                Debug.Log($"[LanternFlowerLightGiver] Enabled LitFlowerB on {flower.name}");
        }

        DisableLightPrompt(flower);
    }

    private void DisableLightPrompt(SproutAndLightManager flower)
    {
        if (flower == null)
            return;

        Transform prompt = FindDeepChildByName(flower.transform, lightPromptObjectName);

        if (prompt == null)
        {
            if (debugLogs)
                Debug.Log($"[LanternFlowerLightGiver] No LightPrompt found on {flower.name}");
            return;
        }

        // Disable common visible/interactive pieces first.
        SpriteRenderer[] renderers = prompt.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = false;
        }

        CanvasGroup[] canvasGroups = prompt.GetComponentsInChildren<CanvasGroup>(true);
        for (int i = 0; i < canvasGroups.Length; i++)
        {
            canvasGroups[i].alpha = 0f;
            canvasGroups[i].interactable = false;
            canvasGroups[i].blocksRaycasts = false;
        }

        Collider2D[] colliders = prompt.GetComponentsInChildren<Collider2D>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        // Finally disable the prompt root itself.
        prompt.gameObject.SetActive(false);

        if (debugLogs)
            Debug.Log($"[LanternFlowerLightGiver] Disabled LightPrompt on {flower.name}");
    }

    private Transform FindDeepChildByName(Transform root, string childName)
    {
        if (root == null)
            return null;

        Transform[] allChildren = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < allChildren.Length; i++)
        {
            if (allChildren[i].name == childName)
                return allChildren[i];
        }

        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lightRadius);
    }
}