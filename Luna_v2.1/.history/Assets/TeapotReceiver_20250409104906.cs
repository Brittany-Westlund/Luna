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

    [Header("Brew Icon UI")]
    public GameObject TeacupBrewIcon; // 🫖 Icon that shows when ready to brew

    [Header("Ingredients")]
    public List<string> teaIngredients = new List<string>();

    private void Start()
    {
        if (TeacupBrewIcon != null)
        {
            TeacupBrewIcon.SetActive(false); // hide by default
        }
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

        // Reset teapot state
        hasLight = false;
        teaIngredients.Clear();
        if (sparkleEffect != null) sparkleEffect.SetActive(false);
        if (TeacupBrewIcon != null) TeacupBrewIcon.SetActive(false); // Hide icon after brewing

        return teacup;
    }

    private void UpdateBrewIcon()
    {
        if (TeacupBrewIcon != null)
        {
            TeacupBrewIcon.SetActive(hasLight); // Only show icon when light is added
        }
    }
}
