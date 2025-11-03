using UnityEngine;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem; // ← required for DialogueManager

public class AdjustBackgroundBrightness : MonoBehaviour
{
    [Header("Settings")]
    public GameObject[] targetBackgrounds;
    public string[] lanternTags;
    [Tooltip("Number of lanterns that need to be lit for full brightness.")]
    public int illuminationsToWhite = 3;

    [Header("Flower Settings")]
    public GameObject[] flowersInScene;
    public AudioSource brightnessFullAudio;

    [Header("Additional Activation")]
    [Tooltip("GameObjects to set active once full brightness is reached.")]
    public GameObject[] objectsToActivate;

    [Tooltip("SpriteRenderers to enable once full brightness is reached.")]
    public SpriteRenderer[] spriteRenderersToEnable;

    [Header("Optional Dialogue Trigger")]
    [Tooltip("If set, will trigger this Dialogue System conversation when full brightness is reached.")]
    public string conversationToStart;
    [Tooltip("Optional actor name (leave blank to use default DialogueManager actor).")]
    public string actorName;

    private HashSet<GameObject> litLanterns = new HashSet<GameObject>();
    private int currentIlluminations = 0;
    private bool isFullyBright = false;
    private bool initialized = false;

    private void Start()
    {
        StartCoroutine(InitializeLanternsAfterDelay(1f));
    }

    private System.Collections.IEnumerator InitializeLanternsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (string tag in lanternTags)
        {
            GameObject[] lanterns = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject lantern in lanterns)
            {
                SpriteRenderer sr = lantern.GetComponent<SpriteRenderer>();
                if (sr != null && sr.enabled)
                    litLanterns.Add(lantern);
            }
        }

        initialized = true;
    }

    private void Update()
    {
        if (!initialized || isFullyBright) return;

        int newlyLitCount = 0;

        // Detect newly lit lanterns
        foreach (string tag in lanternTags)
        {
            GameObject[] lanterns = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject lantern in lanterns)
            {
                SpriteRenderer sr = lantern.GetComponent<SpriteRenderer>();
                if (sr != null && sr.enabled && !litLanterns.Contains(lantern))
                {
                    litLanterns.Add(lantern);
                    newlyLitCount++;
                }
            }
        }

        if (newlyLitCount > 0)
        {
            for (int i = 0; i < newlyLitCount; i++)
            {
                currentIlluminations++;
                ApplyFixedBrightnessStep();

                if (currentIlluminations >= illuminationsToWhite)
                    break;
            }

            if (CheckFullBrightness() || currentIlluminations >= illuminationsToWhite)
            {
                isFullyBright = true;
                TriggerFullBrightnessFeedback();
            }
        }
    }

    private void ApplyFixedBrightnessStep()
    {
        float stepFraction = 1f / Mathf.Max(1, illuminationsToWhite - (currentIlluminations - 1));

        foreach (GameObject bg in targetBackgrounds)
        {
            SpriteRenderer sr = bg.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            Color current = sr.color;
            Color next = Color.Lerp(current, Color.white, stepFraction);
            sr.color = next;
        }
    }

    private bool CheckFullBrightness()
    {
        foreach (GameObject bg in targetBackgrounds)
        {
            SpriteRenderer sr = bg.GetComponent<SpriteRenderer>();
            if (sr == null) continue;
            Color c = sr.color;
            if (c.r < 1f || c.g < 1f || c.b < 1f)
                return false;
        }
        return true;
    }

    private void TriggerFullBrightnessFeedback()
    {
        Debug.Log($"[{name}] ✨ Background fully bright after {currentIlluminations} illuminations!");

        if (brightnessFullAudio != null)
            brightnessFullAudio.Play();

        ActivateFlowers();
        ActivateAdditionalObjects();
        EnableSpriteRenderers();
        TriggerDialogueByName();
    }

    private void ActivateFlowers()
    {
        foreach (GameObject flower in flowersInScene)
        {
            if (flower != null)
                flower.SetActive(true);
        }
    }

    private void ActivateAdditionalObjects()
    {
        foreach (GameObject obj in objectsToActivate)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }

    private void EnableSpriteRenderers()
    {
        foreach (SpriteRenderer sr in spriteRenderersToEnable)
        {
            if (sr != null)
                sr.enabled = true;
        }
    }

    private void TriggerDialogueByName()
{
    if (!string.IsNullOrEmpty(conversationToStart))
    {
        Debug.Log($"[{name}] 🗨️ Triggering conversation: {conversationToStart}");

        if (!string.IsNullOrEmpty(actorName))
        {
            GameObject actorObj = GameObject.Find(actorName);
            if (actorObj != null)
            {
                DialogueManager.StartConversation(conversationToStart, actorObj.transform);
            }
            else
            {
                Debug.LogWarning($"[{name}] Could not find actor '{actorName}' in scene. Starting conversation without actor.");
                DialogueManager.StartConversation(conversationToStart);
            }
        }
        else
        {
            DialogueManager.StartConversation(conversationToStart);
        }
    }
}

}


/*
using UnityEngine;
using System.Collections.Generic;

public class AdjustBackgroundBrightness : MonoBehaviour
{
    [Header("Settings")]
    public GameObject[] targetBackgrounds;      // Backgrounds to brighten
    public string[] lanternTags;                // Lantern tags to watch for
    [Tooltip("Number of lanterns that need to be lit for full brightness.")]
    public int illuminationsToWhite = 3;

    [Header("Flower Settings")]
    public GameObject[] flowersInScene;
    public AudioSource brightnessFullAudio;

    private HashSet<GameObject> litLanterns = new HashSet<GameObject>();
    private int currentIlluminations = 0;
    private bool isFullyBright = false;
    private bool initialized = false;

    private void Start()
    {
        StartCoroutine(InitializeLanternsAfterDelay(1f));
    }

    private System.Collections.IEnumerator InitializeLanternsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Mark all lanterns that start enabled so they don’t trigger immediately
        foreach (string tag in lanternTags)
        {
            GameObject[] lanterns = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject lantern in lanterns)
            {
                SpriteRenderer sr = lantern.GetComponent<SpriteRenderer>();
                if (sr != null && sr.enabled)
                    litLanterns.Add(lantern);
            }
        }

        initialized = true;
    }

    private void Update()
    {
        if (!initialized || isFullyBright) return;

        int newlyLitCount = 0;

        // Detect newly lit lanterns
        foreach (string tag in lanternTags)
        {
            GameObject[] lanterns = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject lantern in lanterns)
            {
                SpriteRenderer sr = lantern.GetComponent<SpriteRenderer>();
                if (sr != null && sr.enabled && !litLanterns.Contains(lantern))
                {
                    litLanterns.Add(lantern);
                    newlyLitCount++;
                }
            }
        }

        if (newlyLitCount > 0)
        {
            for (int i = 0; i < newlyLitCount; i++)
            {
                currentIlluminations++;
                ApplyFixedBrightnessStep();

                if (currentIlluminations >= illuminationsToWhite)
                    break;
            }

            if (CheckFullBrightness() || currentIlluminations >= illuminationsToWhite)
            {
                isFullyBright = true;
                TriggerFullBrightnessFeedback();
            }
        }
    }

    // 🌙 Increase brightness by a fixed fraction each illumination,
    // starting from whatever color you chose in the Inspector
    private void ApplyFixedBrightnessStep()
    {
        // Fraction of the remaining distance to white for even brightening
        float stepFraction = 1f / Mathf.Max(1, illuminationsToWhite - (currentIlluminations - 1));

        foreach (GameObject bg in targetBackgrounds)
        {
            SpriteRenderer sr = bg.GetComponent<SpriteRenderer>();
            if (sr == null) continue;

            Color current = sr.color;
            Color next = Color.Lerp(current, Color.white, stepFraction);
            sr.color = next;
        }
    }

    private bool CheckFullBrightness()
    {
        foreach (GameObject bg in targetBackgrounds)
        {
            SpriteRenderer sr = bg.GetComponent<SpriteRenderer>();
            if (sr == null) continue;
            Color c = sr.color;
            if (c.r < 1f || c.g < 1f || c.b < 1f)
                return false;
        }
        return true;
    }

    private void TriggerFullBrightnessFeedback()
    {
        Debug.Log($"[{name}] ✨ Background fully bright after {currentIlluminations} illuminations!");

        if (brightnessFullAudio != null)
            brightnessFullAudio.Play();

        ActivateFlowers();
    }

    private void ActivateFlowers()
    {
        foreach (GameObject flower in flowersInScene)
        {
            if (flower != null)
                flower.SetActive(true);
        }
    }
}
*/


