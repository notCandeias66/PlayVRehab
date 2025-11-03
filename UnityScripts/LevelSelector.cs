using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelector : MonoBehaviour
{
    [SerializeField] private GameObject levelItemPrefab;  // Your prefab for a single level item UI
    [SerializeField] private Transform contentParent;     // The ScrollView Content transform

    [SerializeField] private GameObject loadLevelButton;

    [SerializeField] private GameObject mainMenu;

    private List<Dictionary<string, object>> userLevels;

    public static Dictionary<string, object> SelectedLevel { get; set; }

    public static LevelItemUI CurrentSelectedItem { get; set; }

    void OnEnable()
    {
        LoadUserLevels();
        PopulateLevels();
        StartCoroutine(CheckSelection());
    }

    void OnDisable()
    {
        // Not sure yet
        SelectedLevel = null;

        StopCoroutine(CheckSelection());
    }

    void LoadUserLevels()
    {
        userLevels = SQLiteManager.GetInstance().GetFilteredData("UserLevels", "user_id", UserSession.UserID);
    }

    void PopulateLevels()
    {
        // Clear previous items
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        foreach (var levelData in userLevels)
        {
            GameObject newItem = Instantiate(levelItemPrefab, contentParent);
            LevelItemUI itemUI = newItem.GetComponent<LevelItemUI>();

            if (itemUI != null)
            {
                itemUI.SetData(levelData);
            }
        }
    }

    private IEnumerator CheckSelection()
    {
        while (true)
        {
            // Show button only when a level is selected
            bool hasSelected = LevelSelector.SelectedLevel != null;
            loadLevelButton.SetActive(hasSelected);

            yield return new WaitForSeconds(0.1f); // Light polling, adjust as needed
        }
    }

    public void BackButton()
    {
        SelectedLevel = null;

        loadLevelButton.SetActive(false);

        if (CurrentSelectedItem != null)
        {
            CurrentSelectedItem.SetSelected(false);
            CurrentSelectedItem = null;
        }

        mainMenu.SetActive(true);
    }

    public void LoadLevel()
    {
        if (LevelSelector.SelectedLevel == null)
        {
            Debug.LogWarning("No level selected!");
            return;
        }

        UserSession.ChoosenLevel = new Dictionary<string, object>(SelectedLevel);

        SelectedLevel = null;

        // Load scene index 1
        UnityEngine.SceneManagement.SceneManager.LoadScene(1);
    }
}
