using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem;

public class TeacupReceiver : MonoBehaviour
{
    private GameObject heldTeacup;

    private GameObject happyIcon;
    private GameObject cozyIcon;
    
    [Header("Dialogue Trigger (drag your DialogueSystemTrigger here)")]
    public DialogueSystemTrigger dialogueTrigger; // Drag in Inspector
   
    [Header("Required Flowers (must all be present to trigger dialogue)")]
    public string[] requiredFlowers;
    private bool hasRequiredIngredients;
    private Transform teacupHoldPoint;

    private void Start()
    {
        // Try to find the hold point and icons by name in children
        teacupHoldPoint = transform.Find("TeacupHoldPoint");
        happyIcon = transform.Find("HappyIcon")?.gameObject;
        cozyIcon = transform.Find("CozyIcon")?.gameObject;

        if (happyIcon != null) happyIcon.SetActive(false);
        if (cozyIcon != null) cozyIcon.SetActive(false);
    }

    public bool CanReceiveTeacup()
    {
        return heldTeacup == null;
    }

    public void ReceiveTeacup(GameObject teacup)
    {
        heldTeacup = teacup;

        if (teacupHoldPoint != null)
        {
            heldTeacup.transform.SetParent(teacupHoldPoint);
            heldTeacup.transform.localPosition = Vector3.zero;
        }
        else
        {
            heldTeacup.transform.SetParent(transform);
            heldTeacup.transform.localPosition = Vector3.zero;
        }

        StartCoroutine(HandleTeacupRoutine());

        var effect = teacup.GetComponent<TeaEffectManager>();
        if (effect != null)
        {
            var ingredients = effect.GetIngredients();
            hasRequiredIngredients = true;
            foreach (string req in requiredFlowers)
            {
                if (!ingredients.Contains(req))
                {
                    hasRequiredIngredients = false;
                    Debug.Log($"Missing required flower: {req}");
                    break;
                }
            }
        }
        else
        {
            Debug.LogWarning("TeaEffectManager not found on teacup—cannot check ingredients!");
            hasRequiredIngredients = false;
        }

        // Fire dialogue if requirements are met
        if (hasRequiredIngredients && dialogueTrigger != null)
        {
            dialogueTrigger.OnUse();
            Debug.Log("DialogueSystemTrigger OnUse called via public field!");
        }
        else if (!hasRequiredIngredients)
        {
            Debug.Log("Required flowers not present—dialogue will not trigger.");
        }
        else
        {
            Debug.LogWarning("No DialogueSystemTrigger assigned in Inspector!");
        }
    }
    private IEnumerator HandleTeacupRoutine()
    {
        yield return new WaitForSeconds(2f); // Hold it briefly

        if (heldTeacup != null)
        {
            Destroy(heldTeacup);
            heldTeacup = null;
        }

        if (happyIcon != null) happyIcon.SetActive(true);
        if (cozyIcon != null) cozyIcon.SetActive(true);

        yield return new WaitForSeconds(60f);

        if (happyIcon != null) happyIcon.SetActive(false);
        if (cozyIcon != null) cozyIcon.SetActive(false);
    }
}
