using UnityEngine;
using System.Collections.Generic;

public class TeapotReceiver : MonoBehaviour
{
    public GameObject teacupPrefab;
    public Transform teacupSpawnPoint;

    public bool hasLight = false;
    public GameObject sparkleEffect;
    public Animator teapotAnimator;

    public List<string> teaIngredients = new List<string>();

    public void ReceiveLight()
    {
        hasLight = true;
        if (sparkleEffect != null) sparkleEffect.SetActive(true);
        if (teapotAnimator != null) teapotAnimator.SetTrigger("Pulse");
        Debug.Log("✨ Teapot charged with light!");
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

        // Reset state
        hasLight = false;
        teaIngredients.Clear();
        if (sparkleEffect != null) sparkleEffect.SetActive(false);

        return teacup;
    }
}
