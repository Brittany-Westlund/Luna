using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int points = 0;
    public Text PointsText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Optionally persist across scenes
        // DontDestroyOnLoad(gameObject);

        if (PointsText != null)
        {
            PointsText.text = "";
        }
        else
        {
            Debug.LogWarning("ScoreManager is missing PointsText reference.");
        }
    }

    public void AddPoint()
    {
        points++;
        UpdatePointsText();
    }

    private void UpdatePointsText()
    {
        if (PointsText != null)
        {
            PointsText.text = points.ToString("D1");
        }
    }
}
