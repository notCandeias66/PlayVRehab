using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndMenu : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI EndMenuObstacleNumber;
    [SerializeField] TextMeshProUGUI EndMenuTimeHolder;
    [SerializeField] TextMeshProUGUI EndMenuScore;
    [SerializeField] Image[] EndMenuStars;

    [SerializeField] ObstacleMovement obstacleMovement;

    public void ShowEndResults(string newLevelString, int obstaclesCompleted, float timeSeconds, int score, int maxScore)
    {
        float completionPercentage = LevelItemUI.GetLevelProgression(newLevelString);

        EndMenuObstacleNumber.text = "Number of Obstacles Completed: " + obstaclesCompleted + " (" + completionPercentage + "% completed)";

        TimeSpan time = TimeSpan.FromSeconds(timeSeconds);
        EndMenuTimeHolder.text = string.Format("Time: {0:mm\\:ss}", time);

        EndMenuScore.text = "Score: " + score.ToString();

        InGameUI.UpdateStars(score, maxScore, EndMenuStars);

        Debug.Log(PauseMenu.FormatAttemptsForStorage(obstacleMovement.levelGenerator.levelString, obstacleMovement.newLevelString, obstacleMovement.levelGenerator.Obstacles).ToString());
    }

    public int GetCompletedObstaclesNumber(string newLevelString)
    {
        string[] codes = newLevelString.Split(',');

        int completed = 0;

        foreach (string code in codes)
        {
            if (code == "!") break;
            
            completed++;
        }
        
        return completed;
    }
}
