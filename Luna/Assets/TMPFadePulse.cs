using UnityEngine;
using TMPro;

public class TMPFadePulse : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float minAlpha = 0.5f;
    public float maxAlpha = 1f;

    private TextMeshProUGUI text;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        Color c = text.color;
        c.a = alpha;
        text.color = c;
    }
}