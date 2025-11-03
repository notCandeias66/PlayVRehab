using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI levelIdText;
    [SerializeField] private TextMeshProUGUI attemptText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timeTakenText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI progressionText;

    [Header("Selection Visual")]
    [SerializeField] private Image selectionImage;

    private Dictionary<string, object> levelData;

    private bool isSelected = false;
    private bool isHovered = false;

    public void SetData(Dictionary<string, object> data)
    {
        levelData = data;

        levelIdText.text = data["level_id"].ToString();
        attemptText.text = data["attempt"].ToString();
        scoreText.text = data["score"].ToString();
        timeTakenText.text = FormatTime(Convert.ToInt32(data["time_taken"]));

        float progress = GetLevelProgression(data["progression"].ToString());
        progressionText.text = progress.ToString("F1") + "%";

        statusText.text = SetStatusText(progress);
        statusText.color = SetStatusColor(statusText.text);

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisual();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateVisual();
    }

    public void OnClick()
    {
        if (LevelSelector.SelectedLevel != null && 
            LevelSelector.SelectedLevel["level_id"].ToString() == levelData["level_id"].ToString() &&
            LevelSelector.SelectedLevel["attempt"].ToString() == levelData["attempt"].ToString())
        {
            return;
        }

        if (LevelSelector.CurrentSelectedItem != null)
        {
            LevelSelector.CurrentSelectedItem.SetSelected(false);
        }

        LevelSelector.CurrentSelectedItem = this;
        LevelSelector.SelectedLevel = levelData;
        SetSelected(true);

        Debug.Log("Selected Level ID: " + levelData["level_id"] + ", Selected Level Attempt: " + levelData["attempt"]);
    }

    private void UpdateVisual()
    {
        if (selectionImage != null)
        {
            selectionImage.enabled = isSelected || isHovered;
        }
    }

    private string FormatTime(int timeInSeconds)
    {
        if (timeInSeconds < 0) return "-";
        
        int minutes = timeInSeconds / 60;
        int seconds = timeInSeconds % 60;

        return $"{minutes:D2}:{seconds:D2}";
    }

    public static float GetLevelProgression(string levelString)
    {
        float progress = 0;

        if (string.IsNullOrEmpty(levelString))
            return progress;

        string[] parts = levelString.Split(',');

        int index = Array.IndexOf(parts, "!");

        if (index == -1 || parts.Length <= 1) return progress;

        int total = parts.Length - 1;

        progress = (float)index / total * 100f;

        return progress;
    }

    public static string SetStatusText(float progressPercentage)
    {
        float roundedProgress = (float)Math.Round(progressPercentage, 1); // Round to 1 decimal

        if (roundedProgress == 0f)
        {
            return "Not Started";
        }
        else if (roundedProgress == 100f)
        {
            return "Completed";
        }
        else
        {
            return "Incompleted";
        }
    }

    private Color SetStatusColor(string status)
    {
        switch (status)
        {
            case ("Not Started"):
                return Color.red;
            case ("Incompleted"):
                return Color.yellow;
            case ("Completed"):
                return Color.green;
            default:
                return Color.white;
        }
    }
}
