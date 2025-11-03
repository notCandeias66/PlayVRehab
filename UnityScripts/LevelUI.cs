using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    [SerializeField] LevelGenerator levelGenerator;
    private ObstacleMovement obstacleMovement;

    [SerializeField] private InGameUI inGameUI;

    [SerializeField] private EndMenu endMenu;

    [SerializeField] private PauseMenu PauseMenu;
    public bool hasPaused = false;

    public GameObject currentObstacle;

    public float elapsedTime = 0f;
    public bool isTimerRunning = false;

    void Start()
    {
        obstacleMovement = GameObject.FindGameObjectWithTag("ObstacleMovement")?.GetComponent<ObstacleMovement>();
        if (obstacleMovement != null)
        {
            obstacleMovement.OnObstacleChanged += OnCurrentObstacleChanged;
            currentObstacle = obstacleMovement.currentObstacle;
        }

        PuppetAvatar.OnPauseTriggered += HandlePauseTriggered;

        StartCoroutine(WaitForLevelGeneration());
    }

    IEnumerator WaitForLevelGeneration()
    {
        while (!levelGenerator.DoneGenerating || obstacleMovement == null || !obstacleMovement.inicialized)
        {
            yield return null; // wait for next frame
        }

        if (UserSession.ChoosenLevel != null)
        {
            if (UserSession.ChoosenLevel.TryGetValue("time_taken", out object timeObject) && timeObject != null)
            {
                int savedTime = Convert.ToInt32(timeObject);

                if (savedTime != -1)
                {
                    elapsedTime = (float)savedTime;
                }
                else
                {
                    elapsedTime = 0f;
                }
            }

            if (UserSession.ChoosenLevel.TryGetValue("score", out object scoreObject) && scoreObject != null)
            {
                int savedScore = Convert.ToInt32(scoreObject);
                inGameUI.score = savedScore;
                inGameUI.ScoreUI.text = savedScore.ToString();

                InGameUI.UpdateStars(inGameUI.score, inGameUI.max_score, inGameUI.StarsUI);
            }
        }

        CountDown.StartCountDown();
        while(CountDown.IsRunning)
        {
            yield return null;
        }

        inGameUI.gameObject.SetActive(true);

        isTimerRunning = true;

        if (obstacleMovement != null)
        {
            OnCurrentObstacleChanged(obstacleMovement.currentObstacle);
        }
    }

    private void OnCurrentObstacleChanged(GameObject newObstacle)
    {
        currentObstacle = newObstacle;
        Debug.Log("Updated currentObstacle in LevelUI: " + (newObstacle != null ? newObstacle.name : null));

        if (currentObstacle == null)
        {
            Debug.Log("Current obstacle is null");
            return;
        }

        // Update the score from the last obstacle upon trasition to a new obstacle
        GameObject lastObstacle = obstacleMovement.lastObstacle;
        if (lastObstacle != null)
        {
            var lastObstacleBase = lastObstacle.GetComponent<ObstacleBase>();
            if (lastObstacleBase != null)
            {
                inGameUI.UpdateScore(obstacleMovement.levelGenerator.Obstacles.Count, lastObstacleBase.attemptsData, lastObstacleBase.ObstacleCode);
            }
        }

        // Check what is the code of the new obstacle
        var obstacleBase = currentObstacle.GetComponent<ObstacleBase>();
        if (obstacleBase == null)
        {
            Debug.LogWarning("Current obstacle missing ObstacleBase component!");
            return;
        }

        string code = obstacleBase.ObstacleCode;

        switch (code)
        {
            case "END":
                inGameUI.gameObject.SetActive(false);
                StartCoroutine(HandleLevelEnd());
                break;

            default:
                break;
        }
    }

    public IEnumerator HandleLevelEnd()
    {
        isTimerRunning = false;

        yield return new WaitForSecondsRealtime(0.05f);

        Time.timeScale = 0f;

        endMenu.gameObject.SetActive(true);

        int obstaclesCompleted = endMenu.GetCompletedObstaclesNumber(obstacleMovement.newLevelString);

        endMenu.ShowEndResults(obstacleMovement.newLevelString, obstaclesCompleted, elapsedTime, inGameUI.score, inGameUI.max_score);
    }

    void Update()
    {
        if (isTimerRunning && Time.timeScale > 0)
        {
            elapsedTime += Time.deltaTime;
            inGameUI.UpdateTime(elapsedTime);
        }
    }

    private void HandlePauseTriggered()
    {
        hasPaused = !hasPaused;

        if (hasPaused)
        {
            Time.timeScale = 0f;

            PauseMenu.gameObject.SetActive(true);
        }
        else
        {
            PauseMenu.gameObject.SetActive(false);

            CountDown.StartCountDown();
        }
    }

    void OnDestroy()
    {
        if (obstacleMovement != null)
            obstacleMovement.OnObstacleChanged -= OnCurrentObstacleChanged;

        PuppetAvatar.OnPauseTriggered -= HandlePauseTriggered;
    }
}
