using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    // Singleton pattern
    public static ScoreManager Instance { get; private set; }

    // Your points and text fields
    public int points = 0;
    public Text PointsText;

    // Singleton setup in Awake
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
         // Set the points text to "00" or just "" depending on what you want
        PointsText.text = ""; // This will show "00"
        // PointsText.text = ""; // This will show nothing
    }

    // Call this to add points
    public void AddPoint()
    {
        points++;
        UpdatePointsText();
    }

    // Update the text UI
    private void UpdatePointsText()
    {
        PointsText.text = points.ToString("D1");
    }
}