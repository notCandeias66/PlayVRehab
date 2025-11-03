using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountDown : MonoBehaviour
{
    private static CountDown _instance;

    public static bool IsRunning { get; private set; }

    [Header("Countdown UI")]
    public GameObject countdown3;
    public GameObject countdown2;
    public GameObject countdown1;
    public GameObject countdownGo;

    private void Awake()
    {
        // Set the singleton instance
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void StartCountDown()
    {
        if (_instance != null && !IsRunning)
        {
            _instance.StartCoroutine(_instance.CountdownRoutine());
        }
        else
        {
            Debug.LogWarning("CountDown instance missing or already running.");
        }
    }

    private IEnumerator CountdownRoutine()
    {
        IsRunning = true;

        // Freeze game logic
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(0.5f);

        _instance.countdown3.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        _instance.countdown3.SetActive(false);

        _instance.countdown2.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        _instance.countdown2.SetActive(false);

        _instance.countdown1.SetActive(true);
        yield return new WaitForSecondsRealtime(1f);
        _instance.countdown1.SetActive(false);

        _instance.countdownGo.SetActive(true);
        yield return new WaitForSecondsRealtime(2f);
        _instance.countdownGo.SetActive(false);

        yield return new WaitForSecondsRealtime(1f);

        // Resume gameplay
        Time.timeScale = 1f;

        IsRunning = false;
    }
}
