using UnityEngine;

public class LightMotePulsate : MonoBehaviour
{
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public float pulseSpeed = 2f;

    private SpriteRenderer spriteRenderer;
    private float scaleDifference;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on the object.");
            this.enabled = false;
            return;
        }

        // Initialize alpha to 1 (fully opaque)
        Color color = spriteRenderer.color;
        color.a = 1f;
        spriteRenderer.color = color;

        

        scaleDifference = maxScale - minScale;
    }

    void Update()
    {
        // Pulsate scale
        float scale = minScale + Mathf.PingPong(Time.time * pulseSpeed, scaleDifference);
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
