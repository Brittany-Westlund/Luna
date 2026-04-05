using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class UniversalTextFadeInAfterDelay : MonoBehaviour
{
    [Header("Text References (Assign One)")]
    public TextMeshProUGUI tmpUGUI;
    public TextMeshPro tmp3D;
    public Text uiText;

    [Header("Timing")]
    public float delay = 1f;
    public float fadeDuration = 1f;

    void Start()
    {
        AutoAssignIfNeeded();

        if (!HasValidText()) return;

        SetAlpha(0f);
        StartCoroutine(FadeInRoutine());
    }

    private void AutoAssignIfNeeded()
    {
        if (tmpUGUI == null && tmp3D == null && uiText == null)
        {
            tmpUGUI = GetComponent<TextMeshProUGUI>();
            tmp3D = GetComponent<TextMeshPro>();
            uiText = GetComponent<Text>();
        }
    }

    private bool HasValidText()
    {
        return tmpUGUI != null || tmp3D != null || uiText != null;
    }

    private IEnumerator FadeInRoutine()
    {
        yield return new WaitForSeconds(delay);

        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, time / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(1f);
    }

    private void SetAlpha(float alpha)
    {
        if (tmpUGUI != null)
        {
            Color c = tmpUGUI.color;
            c.a = alpha;
            tmpUGUI.color = c;
        }

        if (tmp3D != null)
        {
            Color c = tmp3D.color;
            c.a = alpha;
            tmp3D.color = c;
        }

        if (uiText != null)
        {
            Color c = uiText.color;
            c.a = alpha;
            uiText.color = c;
        }
    }
}