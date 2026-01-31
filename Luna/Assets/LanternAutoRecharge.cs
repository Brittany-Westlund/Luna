using UnityEngine;
using System.Collections;

public class LanternAutoRecharge : MonoBehaviour
{
    [Header("Lantern Visual")]
    public SpriteRenderer litLanternSprite;

    [Header("Recharge Settings")]
    public float rechargeDelay = 6f;
    public float fadeDuration = 1.5f;

    private bool isRecharging = false;

    void Update()
    {
        if (litLanternSprite == null) return;

        // If lantern is OFF and not already recharging → start recharge
        if (!litLanternSprite.enabled && !isRecharging)
        {
            StartCoroutine(RechargeRoutine());
        }
    }

    private IEnumerator RechargeRoutine()
    {
        isRecharging = true;

        yield return new WaitForSeconds(rechargeDelay);

        // Fade back in
        litLanternSprite.enabled = true;
        yield return StartCoroutine(FadeIn());

        isRecharging = false;
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        Color c = litLanternSprite.color;
        c.a = 0f;
        litLanternSprite.color = c;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            litLanternSprite.color = c;
            yield return null;
        }

        c.a = 1f;
        litLanternSprite.color = c;
    }
}
