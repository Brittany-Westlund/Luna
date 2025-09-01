using UnityEngine;

public class TeacupHighlight : MonoBehaviour
{
    public Color highlightColor = Color.yellow; // You can set this in Inspector!
    private Color _originalColor;
    private SpriteRenderer _sr;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null)
            _originalColor = _sr.color;
    }

    public void Highlight()
    {
        if (_sr != null)
            _sr.color = highlightColor;
    }

    public void RemoveHighlight()
    {
        if (_sr != null)
            _sr.color = _originalColor;
    }
}
