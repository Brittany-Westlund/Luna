using UnityEngine;

public class GrassGlowManager : MonoBehaviour
{
    public SpriteRenderer grassRenderer;
    public Color litColor = Color.white;
    public Color unlitColor = new Color(1f, 1f, 1f, 0.4f); // or whatever unlit is

    private static GrassGlowManager currentlyLitGrass;

    private void Start()
    {
        SetLit(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (currentlyLitGrass != null && currentlyLitGrass != this)
            {
                currentlyLitGrass.SetLit(false);
            }

            SetLit(true);
            currentlyLitGrass = this;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && currentlyLitGrass == this)
        {
            SetLit(false);
            currentlyLitGrass = null;
        }
    }

    public void SetLit(bool isLit)
    {
        if (grassRenderer != null)
        {
            grassRenderer.color = isLit ? litColor : unlitColor;
        }
    }
}
