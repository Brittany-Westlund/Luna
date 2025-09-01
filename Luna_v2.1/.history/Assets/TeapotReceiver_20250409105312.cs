using UnityEngine;
using System.Collections.Generic;

public class TeapotReceiver : MonoBehaviour
{
    [Header("Teacup Settings")]
    public GameObject teacupPrefab;
    public Transform teacupSpawnPoint;

    [Header("Light & Sparkle")]
    public bool hasLight = false;
    public GameObject sparkleEffect;
    public Animator teapotAnimator;

    [Header("Ingredients")]
    public List<string> teaIngredients = new List<string>();

    // Private reference to icon
    private GameObject TeacupBrewIcon;

    private void Start()
    {
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            if (child.name == "TeacupBrewIcon")
            {
                TeacupBrewIcon = child.gameObject;
                TeacupBrewIcon.SetActive(false);
                Debug.Log("✅ Found TeacupBrewIcon in children.");
                return;
            }
        }

        Debug.LogWarning("⚠️ TeacupBrewIcon child not found after full search.");
    }


    public void ReceiveLight()
    {
        hasLight = true;
        if (sparkleEffect != null) sparkleEffect.SetActive(true);
        if (teapotAnimator != null) teapotAnimator.SetTrigger("Pulse");
        Debug.Log("✨ Teapot charged with light!");
        UpdateBrewIcon();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("LightMote"))
        {
            ReceiveLight();
            Destroy(other.gameObject);
        }
    }

    public GameObject BrewTea()
    {
        if (!hasLight)
        {
            Debug.LogWarning("❌ Can't brew without light.");
            return null;
        }

        GameObject teacup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        TeaEffectManager effect = teacup.GetComponent<TeaEffectManager>();
        if (effect != null)
        {
            effect.SetIngredients(teaIngredients, hasLight);
        }

        hasLight = false;
        teaIngredients.Clear();
        if (sparkleEffect != null) sparkleEffect.SetActive(false);
        if (TeacupBrewIcon != null) TeacupBrewIcon.SetActive(false);

        return teacup;
    }

    private void UpdateBrewIcon()
    {
        if (TeacupBrewIcon != null)
        {
            TeacupBrewIcon.SetActive(hasLight);
        }
    }
}
