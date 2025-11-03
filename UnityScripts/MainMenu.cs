using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

using Microsoft.Azure.Kinect.Sensor;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI welcomeUserText;
    [SerializeField] private TextMeshProUGUI gameVersionText;

    [SerializeField] private Button kinectStatusButton;
    [SerializeField] private Button userStatusButton;

    [SerializeField] private Sprite yesAuth;
    [SerializeField] private Sprite noAuth;

    [SerializeField] private Sprite yesConn;
    [SerializeField] private Sprite noConn;

    [SerializeField] private GameObject levelSelectorUI;

    private Coroutine kinectCheckCoroutine;


    void OnEnable()
    {
        // Stop the coroutine if it's already running
        if (kinectCheckCoroutine != null)
        {
            StopCoroutine(kinectCheckCoroutine);
            kinectCheckCoroutine = null;
        }

        gameVersionText.text = "v1.00";

        // Automatically runs when the scene loads
        if (IsUserAuthenticated())
        {
            Debug.Log("Authenticated User: ID = " + UserSession.UserID + ", Name = " + UserSession.UserName);

            welcomeUserText.text = "Welcome, " + UserSession.UserName;

            userStatusButton.GetComponent<UnityEngine.UI.Image>().sprite = yesAuth;
        }
        else
        {
            welcomeUserText.text = "Welcome,";

            userStatusButton.GetComponent<UnityEngine.UI.Image>().sprite = noAuth;
        }

        // Start the coroutine and store the reference
        kinectCheckCoroutine = StartCoroutine(CheckKinectConnection());
    }

    void OnDisable()
    {
        if (kinectCheckCoroutine != null)
        {
            StopCoroutine(kinectCheckCoroutine);
            kinectCheckCoroutine = null;
        }
    }

    public void PlayGame()
    {
        if (IsUserAuthenticated() && Device.GetInstalledCount() == 1)
        {
            gameObject.SetActive(false);
            levelSelectorUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("User not authenticated or Kinect not connected.");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private bool IsUserAuthenticated()
    {
        return UserSession.UserID != 0 && !string.IsNullOrEmpty(UserSession.UserName);
    }

    private IEnumerator CheckKinectConnection()
    {
        // Repeat every 1 second (you can adjust this frequency as needed)
        while (true)
        {
            try
            {
                int sensors = Device.GetInstalledCount();

                if (kinectStatusButton != null)
                {
                    if (sensors == 1)
                    {
                        // Set the Kinect status button to the connected state
                        kinectStatusButton.GetComponent<UnityEngine.UI.Image>().sprite = yesConn;
                    }
                    else
                    {
                        // Set the Kinect status button to the disconnected state
                        kinectStatusButton.GetComponent<UnityEngine.UI.Image>().sprite = noConn;
                    }
                }
            } 
            catch (System.Exception)
            {
                // Handle any exception (e.g., Kinect not found)
                kinectStatusButton.GetComponent<UnityEngine.UI.Image>().sprite = noConn;
                Debug.LogError("Error checking Kinect device connection.");
            }

            yield return new WaitForSeconds(1f); // Wait 1 second before checking again
        }
    }

}
