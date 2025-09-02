using UnityEngine;

public class Pulsate : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float scaleAmount = 0.1f;
    public float fadeAmount = 0.5f;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Color originalColor;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Pulsate script requires a SpriteRenderer component");
            enabled = false;
            return;
        }

        originalScale = transform.localScale;
        originalColor = spriteRenderer.color;
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the scale and fade factor based on a Sin wave
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * scaleAmount + 1;
        float fade = Mathf.Sin(Time.time * pulseSpeed) * fadeAmount + 1 - fadeAmount;

        // Apply the scale and fade changes
        transform.localScale = originalScale * pulse;

        Color newColor = originalColor;
        newColor.a = fade;
        spriteRenderer.color = newColor;
    }
}
