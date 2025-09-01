using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int points = 0;
    private Text PointsText;
    private bool hasAssignedPointsText = false;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadPoints(); // Load saved score FIRST!
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        if (!hasAssignedPointsText)
        {
            var pointsTextObj = GameObject.FindWithTag("PointsText");
            if (pointsTextObj != null)
            {
                PointsText = pointsTextObj.GetComponent<Text>();
                UpdatePointsText();
                hasAssignedPointsText = true;
            }
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
        var pointsTextObj = GameObject.FindWithTag("PointsText");
        PointsText = pointsTextObj != null ? pointsTextObj.GetComponent<Text>() : null;
        UpdatePointsText(); // Update the UI with the CORRECT points value
        hasAssignedPointsText = false;
        if (PointsText != null) PointsText.text = points.ToString();

    }

    public void AddPoint()
    {
        points++;
        UpdatePointsText();
        SavePoints();
    }

    public void UpdatePointsText()
    {
        if (PointsText != null)
        {
            PointsText.text = points.ToString("D1");
            PointsText.enabled = true;
        }
    }

    public void SavePoints()
    {
        PlayerPrefs.SetInt("PlayerPoints", points);
        PlayerPrefs.Save();
    }

    public void LoadPoints()
    {
        if (PlayerPrefs.HasKey("PlayerPoints"))
        {
            points = PlayerPrefs.GetInt("PlayerPoints");
        }
        else
        {
            points = 0;
        }
    }

    public void RegisterPointsText(Text newText)
    {
        PointsText = newText;
        UpdatePointsText();
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