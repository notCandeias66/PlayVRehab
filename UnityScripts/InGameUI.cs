using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI TimeHolderUI;
    [SerializeField] public TextMeshProUGUI ScoreUI;
    [SerializeField] public Image[] StarsUI;

    public int score = 0;
    public int max_score = 10000;

    // Functions that will be used by ObstacleMovement to update scores and stars
    public void UpdateScore(int numberObstacles, Dictionary<int, string> attemptData, string obstacleCode)
    {
        if (obstacleCode == "!" || obstacleCode == "END") return;

        int attempts = attemptData.Count;

        int scoreToAdd = 0;
        int baseScore = max_score / (numberObstacles - 2);

        /*switch (attempts)
        {
            case 0:
                scoreToAdd = 0;
                break;

            default:
                if (obstacleCode.StartsWith("C"))
                {
                    //baseScore = 800;
                }
                else if (obstacleCode.StartsWith("J"))
                {
                    //baseScore = 900;
                }
                else if (obstacleCode.StartsWith("LH"))
                {
                    //baseScore = 1000;
                }
                else
                {
                    baseScore = 0;
                }

                scoreToAdd = Mathf.Max(0, baseScore - (attempts - 1) * 250);
                break;
        }*/

        scoreToAdd = Mathf.Max(0, baseScore - (attempts - 1) * 250);

        score += scoreToAdd;

        UpdateStars(score, max_score, StarsUI);

        ScoreUI.text = "Score: " + score;
    }

    public static void UpdateStars(int score, int max_score, Image[] StarsUI)
    {
        int starCount = StarsUI.Length;
        int starScore = max_score / starCount;
        int starsToFill = Mathf.Clamp(score / starScore, 0, starCount);

        for (int i = 0; i < starCount; i++)
        {
            StarsUI[i].color = i < starsToFill ? Color.yellow : Color.gray;
        }
    }

    public void UpdateTime(float time)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);

        TimeHolderUI.text = string.Format("{0:mm\\:ss}", timeSpan);
    }
}
