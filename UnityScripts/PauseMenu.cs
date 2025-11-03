using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] Button SaveAndQuitButton;
    [SerializeField] Button QuitWithoutSavingButton;

    [SerializeField] GameObject WarningScreenPopUp;
    [SerializeField] TextMeshProUGUI WarningText;
    [SerializeField] Button YesButton;
    [SerializeField] Button NoButton;

    [SerializeField] public GameObject EndMenu;

    [SerializeField] public InGameUI inGameUI;

    [SerializeField] public LevelUI levelUI;

    public int clickedButton = 0; // -1 for quiting without saving | 1 for save and quit | 0 for default value

    void OnEnable()
    {
        Time.timeScale = 0f;
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
    }

    public void QuitWithoutSavingButtonClick()
    {
        clickedButton = -1;

        DisplayCorrectPopUp();

        Time.timeScale = 0f;
    }

    public void SaveAndQuitButtonClick()
    {
        clickedButton = 1;

        DisplayCorrectPopUp();
    }

    public void DisplayCorrectPopUp()
    {
        WarningScreenPopUp.SetActive(true);

        if (clickedButton == -1) // Quit without saving
        {
            SaveAndQuitButton.gameObject.SetActive(false);

            QuitWithoutSavingButton.enabled = false;

            string withoutSavingColored = "<color=#FF0000>without saving</color>";
            WarningText.text = "Are you sure you want to quit the level " + withoutSavingColored + "? All made progress will be lost!";
        }
        else if (clickedButton == 1) // Save and quit
        {
            QuitWithoutSavingButton.gameObject.SetActive(false);

            SaveAndQuitButton.enabled = false;

            string willBeSavedColored = "<color=#00FF00>will be saved</color>";
            WarningText.text = "Your progress on the level " + willBeSavedColored + "!";
        }
    }

    public void NoButtonClicked()
    {
        WarningScreenPopUp.SetActive(false);

        QuitWithoutSavingButton.gameObject.SetActive(true);
        SaveAndQuitButton.gameObject.SetActive(true);

        QuitWithoutSavingButton.enabled = true;
        SaveAndQuitButton.enabled = true;

        clickedButton = 0;
    }

    public void YesButtonClicked()
    {
        if (clickedButton == -1) // Quit without saving
        {
            QuitToMainMenu();
        }
        else if (clickedButton == 1) // Save and quit
        {
            SaveGame();
            QuitToMainMenu();
        }
    }

    public void SaveGame()
    {
        Dictionary<string, object> levelData = UserSession.ChoosenLevel;
        if (levelData == null)
        {
            Debug.LogError("No level selected in UserSession.");
            return;
        }

        if (Convert.ToInt32(levelData["user_id"]) != UserSession.UserID || Convert.ToInt32(levelData["user_id"]) <= 0)
        {
            Debug.LogError("User ID mismatch or invalid.");
            return;
        }

        int userId = Convert.ToInt32(levelData["user_id"]);
        int levelId = Convert.ToInt32(levelData["level_id"]);
        int attemptNumber = Convert.ToInt32(levelData["attempt"]);

        ObstacleMovement obstacleMovement = FindObjectOfType<ObstacleMovement>();
        if (obstacleMovement == null) 
        {
            Debug.LogError("ObstacleMovement script not found in scene.");
            return;
        }

        string progressionString = obstacleMovement.newLevelString;
        List<GameObject> completedObstacles = obstacleMovement.levelGenerator.Obstacles;

        string game_results = FormatAttemptsForStorage(obstacleMovement.levelGenerator.levelString, progressionString, completedObstacles);

        int score = inGameUI.score;

        int time_taken = Mathf.RoundToInt(levelUI.elapsedTime);

        string status = LevelItemUI.SetStatusText(LevelItemUI.GetLevelProgression(progressionString));

        SQLiteManager.GetInstance().UpdateUserLevelData(userId, levelId, attemptNumber, score, time_taken, status, game_results, progressionString);
    }

    public void QuitToMainMenu()
    {
        UserSession.ChoosenLevel = null;

        gameObject.SetActive(false);

        EndMenu.SetActive(false);

        // Load scene index 0
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    // Supporting Functions

    // Function to format the attemps data for storage and better readability
    public static string FormatAttemptsForStorage(string originalLevelString, string currentLevelString, List<GameObject> obstacles)
    {
        if (string.IsNullOrEmpty(currentLevelString) || obstacles == null || obstacles.Count == 0)
            return "No obstacles completed.";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // Step 1: Add previous results from UserSession
        if (UserSession.ChoosenLevel != null &&
            UserSession.ChoosenLevel.TryGetValue("game_results", out object previousResults) &&
            previousResults != null && previousResults.ToString() != "No results!")
        {
            sb.AppendLine(previousResults.ToString());
            sb.AppendLine(); // Space between old and new results
        }
        else
        {
            // No previous results — add your header
            sb.AppendLine("Game Results:");
            sb.AppendLine();
        }

        // Step 2: Determine how many obstacles were completed
        string[] originalCodes = originalLevelString.Split(',');
        string[] currentCodes = currentLevelString.Split(',');

        int originalBangIndex = Array.IndexOf(originalCodes, "!");
        int currentBangIndex = Array.IndexOf(currentCodes, "!");

        // Defensive checks if '!' not found
        if (originalBangIndex == -1 || currentBangIndex == -1) return null;

        HashSet<GameObject> usedObstacles = new HashSet<GameObject>();

        // Iterate over new obstacles completed after the last '!' in original up to before the current '!'
        for (int i = originalBangIndex; i <= currentBangIndex; i++)
        {
            string code = currentCodes[i];

            GameObject matched = obstacles.Find(obs =>
            {
                if (usedObstacles.Contains(obs)) return false;

                ObstacleBase obsScript = obs.GetComponent<ObstacleBase>();
                if (obsScript == null) return false;

                // Extract alphabetic part of the code
                string obsCodePrefix = Regex.Match(obsScript.ObstacleCode, @"^[A-Za-z]+").Value;
                string codePrefix = Regex.Match(code, @"^[A-Za-z]+").Value;

                return obsCodePrefix == codePrefix;
            });

            if (matched == null)
            {
                Debug.LogWarning($"No matching obstacle found for code: {code}");
                continue;
            }

            usedObstacles.Add(matched);

            ObstacleBase matchedScript = matched.GetComponent<ObstacleBase>();
            if (matchedScript == null || matchedScript.attemptsData == null || matchedScript.attemptsData.Count == 0)
                continue;

            sb.AppendLine($"Obstacle Code: " + matchedScript.ObstacleCode);

            foreach (var attempt in SortedAttempts(matchedScript.attemptsData))
            {
                sb.AppendLine($"- Attempt {attempt.Key + 1}");
                sb.AppendLine(attempt.Value);
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    // Function to get ordinal so it is easier to know obstacle order
    private static string GetOrdinal(int number)
    {
        if (number % 100 >= 11 && number % 100 <= 13)
            return number + "th";

        switch (number % 10)
        {
            case 1: return number + "st";
            case 2: return number + "nd";
            case 3: return number + "rd";
            default: return number + "th";
        }
    }

    // Function to order obstacle attmpts data by the attempt number (probably not needed)
    private static List<KeyValuePair<int, string>> SortedAttempts(Dictionary<int, string> attempts)
    {
        var sorted = new List<KeyValuePair<int, string>>(attempts);
        sorted.Sort((a, b) => a.Key.CompareTo(b.Key));
        return sorted;
    }


}