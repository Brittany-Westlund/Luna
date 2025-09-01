using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int points = 0;
    private Text PointsText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Try to find PointsText by tag each time a new scene loads
        var pointsTextObj = GameObject.FindWithTag("PointsText");
        PointsText = pointsTextObj != null ? pointsTextObj.GetComponent<Text>() : null;
        UpdatePointsText();
    }

    public void AddPoint()
    {
        points++;
        UpdatePointsText();
    }

    public void UpdatePointsText()
    {
        if (PointsText != null)
        {
            PointsText.text = points.ToString("D1");
            PointsText.enabled = true;
        }
    }
}

/* using UnityEngine;
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
        DontDestroyOnLoad(gameObject);

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

    public void UpdatePointsText()
    {
        if (PointsText != null)
        {
            PointsText.text = points.ToString("D1");
            PointsText.enabled = true; // <-- This line ensures PointsText visible!
        }
    }

    // In ScoreManager
    public void FindAndAssignPointsText()
    {
        PointsText = GameObject.FindWithTag("PointsText")?.GetComponent<Text>();
        UpdatePointsText();
    }

}
*/