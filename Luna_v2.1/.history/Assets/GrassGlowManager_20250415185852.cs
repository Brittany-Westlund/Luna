using UnityEngine;

public class GrassGlowManager : MonoBehaviour
{
    public SpriteRenderer grassRenderer;
    public Color litColor = Color.white;
    public Color unlitColor = new Color(1f, 1f, 1f, 0.4f);

    private static GrassGlowManager currentlyLit;
    private bool isLunaInside = false;

    private void Start()
    {
        SetLit(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isLunaInside = true;

            if (currentlyLit != null && currentlyLit != this)
            {
                currentlyLit.SetLit(false);
                currentlyLit.isLunaInside = false;
            }

            currentlyLit = this;
            SetLit(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isLunaInside = false;

            // Only unlight this grass if it’s the active one
            if (currentlyLit == this)
            {
                SetLit(false);
                currentlyLit = null;
            }
        }
    }

    public void SetLit(bool lit)
    {
        if (grassRenderer != null)
        {
            grassRenderer.color = lit ? litColor : unlitColor;
        }
    }
}
