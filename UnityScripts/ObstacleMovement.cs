using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;

public class ObstacleMovement : MonoBehaviour
{
    [Header("External References")]
    public LevelGenerator levelGenerator;
    public PuppetAvatar puppetAvatar;
    public LevelUI levelUI;

    [Header("Movement Settings")]
    public float movementSpeed = 1f;
    public float wallPushThreshold = 0.2f;
    public float retreatDistance = 1f;
    public float legHoldRetreatSpeed = 1f;
    public float obstacleStopDistance = 0.5f;

    public float nextObstacleDistance = 0.2f;

    [Header("Runtime State")]
    public string newLevelString;
    public int currentObstaclePos;

    public GameObject _currentObstacle;
    public GameObject currentObstacle
    {
        get => _currentObstacle;
        private set
        {
            if (_currentObstacle != value)
            {
                _currentObstacle = value;
                OnObstacleChanged?.Invoke(_currentObstacle);
            }
        }
    }

    public GameObject lastObstacle;
    private GameObject player;
    private float courseLength;

    private float legHoldTimer = 0f;
    private int requiredHoldTime = 0;
    private bool hasPassedLegHold = false;
    private float legHoldAccumulatedTime = 0f;

    private Coroutine legHoldRoutine;

    public delegate void ObstacleChangedHandler(GameObject newObstacle);
    public event ObstacleChangedHandler OnObstacleChanged;

    public bool inicialized = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player) puppetAvatar = player.GetComponent<PuppetAvatar>();

        levelUI = FindObjectOfType<LevelUI>();

        StartCoroutine(WaitForObstacles());
    }

    IEnumerator WaitForObstacles()
    {
        while (!levelGenerator.DoneGenerating)
            yield return null;

        newLevelString = levelGenerator.levelString;

        int spawnIndex = levelGenerator.Obstacles.FindIndex(
            obs => obs.GetComponent<ObstacleBase>()?.ObstacleCode == "!"
        );

        currentObstaclePos = spawnIndex + 1;
        currentObstacle = levelGenerator.Obstacles[currentObstaclePos];

        if (spawnIndex == 0)
        {
            lastObstacle = null;
        }
        else
        {
            lastObstacle = levelGenerator.Obstacles[spawnIndex - 1];
        }

        courseLength = levelGenerator.Obstacles.Last().transform.position.z;

        inicialized = true;
    }

    void Update()
    {
        if (!inicialized || !levelUI.isTimerRunning || player == null || currentObstacle == null || levelUI.hasPaused) return;

        HandleWallProximityPush(); // Think it is working

        if (!hasPassedLegHold && IsLegHoldObstacle(currentObstacle) && IsPlayerOnTopObstacle() && IsPlayerRoughlyAtObstacleCenter())
        {
            if (legHoldRoutine == null)
            {
                legHoldRoutine = StartCoroutine(HandleLegHoldObstacle());
            }
        }
        else
        {
            if (IsLegHoldObstacle(lastObstacle))
            {
                ResetLegHoldState();
            }

            MoveObstacles(Vector3.back * movementSpeed * Time.deltaTime);
        }

        HandleObstacleTransition();
    }

    void HandleWallProximityPush()
    {
        foreach (var pCol in player.GetComponentsInChildren<Collider>())
        {
            // Top right side of the colliders
            Vector3 topRight = new Vector3(pCol.bounds.min.x, pCol.bounds.max.y, pCol.bounds.min.z);

            // Top left side of the colliders
            Vector3 topLeft = new Vector3(pCol.bounds.max.x, pCol.bounds.max.y, pCol.bounds.min.z);

            // Middle right side of the colliders
            Vector3 middleRight = new Vector3(pCol.bounds.min.x, pCol.bounds.center.y, pCol.bounds.min.z);

            // Middle left side of the colliders
            Vector3 middleLeft = new Vector3(pCol.bounds.max.x, pCol.bounds.center.y, pCol.bounds.min.z);

            //Debug.DrawRay(topRight, Vector3.back * wallPushThreshold, Color.cyan, wallPushThreshold);

            //Debug.DrawRay(topLeft, Vector3.back * wallPushThreshold, Color.cyan, wallPushThreshold);

            //Debug.DrawRay(middleRight, Vector3.back * wallPushThreshold, Color.cyan, wallPushThreshold);

            //Debug.DrawRay(middleLeft, Vector3.back * wallPushThreshold, Color.cyan, wallPushThreshold);

            RaycastHit hit;
            int layerMask = ~LayerMask.GetMask("Character");

            bool hitDetected = Physics.Raycast(topRight, Vector3.back, out hit, wallPushThreshold, layerMask) ||
                Physics.Raycast(topLeft, Vector3.back, out hit, wallPushThreshold, layerMask) ||
                Physics.Raycast(middleRight, Vector3.back, out hit, wallPushThreshold, layerMask) ||
                Physics.Raycast(middleLeft, Vector3.back, out hit, wallPushThreshold, layerMask);

            if (hitDetected)
            {
                Debug.Log("Hit Detected = " + hitDetected + ", object = " + hit.collider.gameObject);

                if (GetParentObstacle(hit.collider.gameObject) != null)
                {
                    StartCoroutine(WallRetreat());
                    break;
                }
            }
        }
    }

    IEnumerator WallRetreat()
    {
        List<GameObject> targets = levelGenerator.Obstacles.Skip(currentObstaclePos).ToList();
        List<Vector3> originalPos = targets.Select(o => o.transform.position).ToList();

        float duration = retreatDistance / movementSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            for (int i = 0; i < targets.Count; i++)
                targets[i].transform.position = Vector3.Lerp(originalPos[i], originalPos[i] + Vector3.back * retreatDistance, elapsed / duration);

            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < targets.Count; i++)
            targets[i].transform.position = originalPos[i] + Vector3.back * retreatDistance;
    }

    void MoveObstacles(Vector3 delta)
    {
        float stopZ = -courseLength - 5f;

        foreach (var obs in levelGenerator.Obstacles)
        {
            if (!obs.activeSelf) continue;
            if (obs.transform.position.z >= stopZ)
                obs.SetActive(false);
            else
                obs.transform.Translate(delta);
        }
    }

    void HandleObstacleTransition()
    {
        Collider playerCol = player.GetComponent<Collider>();
        Vector3 down = new Vector3(playerCol.bounds.center.x, playerCol.bounds.min.y + 0.1f, playerCol.bounds.center.z);

        //Debug.DrawRay(down, Vector3.down, Color.yellow, 5f);

        int layerMask = ~LayerMask.GetMask("Character");
        if (Physics.Raycast(down, Vector3.down, out RaycastHit hit, 5f, layerMask))
        {
            GameObject newObstacle = GetParentObstacle(hit.collider.gameObject);

            if (newObstacle != currentObstacle && newObstacle.GetComponent<ObstacleBase>().ObstacleCode != "!")
            {
                // Normal obstacle logic for obstacles in the list
                if (levelGenerator.Obstacles.Contains(newObstacle))
                {
                    // Update attempt data based on obstacle code
                    if (currentObstacle != null)
                    {
                        var currentScript = currentObstacle.GetComponent<ObstacleBase>();
                        if (currentScript != null && puppetAvatar != null)
                        {
                            if (currentScript.ObstacleCode.StartsWith("C"))
                            {
                                currentScript.attemptsData = puppetAvatar.GetAndClearActionInfo(puppetAvatar.crouchInfo);
                            }
                            else if (currentScript.ObstacleCode.StartsWith("J"))
                            {
                                currentScript.attemptsData = puppetAvatar.GetAndClearActionInfo(puppetAvatar.jumpInfo);
                            }
                            else if (currentScript.ObstacleCode.StartsWith("L"))
                            {
                                currentScript.attemptsData = puppetAvatar.GetAndClearActionInfo(puppetAvatar.holdInfo);
                            }
                            else if (currentScript.ObstacleCode.StartsWith("!"))
                            {
                                currentScript.attemptsData = null;
                            }
                            else if (currentScript.ObstacleCode == "END")
                            {
                                currentScript.attemptsData = null;

                                Debug.Log("Player reached END obstacle!");
                            }
                        }
                    }

                    // Advance obstacle tracking
                    lastObstacle = currentObstacle;
                    currentObstacle = newObstacle;

                    currentObstaclePos++;

                    UpdateLevelStringProgress();

                    // Run gimmick if still within obstacles list range
                    if (currentObstaclePos < levelGenerator.Obstacles.Count)
                    {
                        ResetLegHoldState();

                        var script = currentObstacle.GetComponent<ObstacleBase>();
                        script?.RunGimmick();
                    }
                }
            }
        }
    }

    IEnumerator HandleLegHoldObstacle()
    {
        var script = currentObstacle.GetComponent<ObstacleBase>();
        if (script == null) yield break;

        requiredHoldTime = script.GetDelayValue();

        float lastHoldEndTime = 0f;
        legHoldAccumulatedTime = 0f;

        while (!hasPassedLegHold)
        {
            if (IsCorrectLegHeld(script))
            {
                if (lastHoldEndTime != 0f)
                {
                    float breakDuration = Time.time - lastHoldEndTime;
                    legHoldAccumulatedTime -= breakDuration;
                    legHoldAccumulatedTime = Mathf.Max(0f, legHoldAccumulatedTime);

                    lastHoldEndTime = 0f;
                }

                legHoldAccumulatedTime += Time.deltaTime;

                MoveNextObstacles(Vector3.back * movementSpeed * Time.deltaTime);

                if (legHoldAccumulatedTime >= requiredHoldTime)
                {
                    hasPassedLegHold = true;
                    break;
                }
            }
            else
            {
                lastHoldEndTime = Time.time;

                yield return StartCoroutine(MoveNextObstaclesBack(script));
            }

            yield return null;
        }

        legHoldAccumulatedTime = 0f;
    }

    void MoveNextObstacles(Vector3 delta)
    {
        // For all obstacles that come after the current one
        foreach (GameObject obs in levelGenerator.Obstacles.Skip(currentObstaclePos + 1))
        {
            obs.transform.Translate(delta);
        }
    }

    IEnumerator MoveNextObstaclesBack(ObstacleBase script)
    {
        int gap = script.GetDelayValue();

        List<GameObject> targets = levelGenerator.Obstacles.Skip(currentObstaclePos + 1).ToList();

        while (!IsCorrectLegHeld(script) && !hasPassedLegHold)
        {
            foreach (var obs in targets)
            {              
                float curZ = obs.transform.position.z;
                float targetZ = currentObstacle.transform.position.z 
                    - (currentObstacle.GetComponent<ObstacleBase>().GetLength() / 2) 
                    - gap 
                    - (obs.GetComponent<ObstacleBase>().GetLength() / 2);

                if (curZ > targetZ)
                {
                    obs.transform.Translate(Vector3.forward * legHoldRetreatSpeed * Time.deltaTime);
                }
            }

            yield return null;
        }
    }

    private bool IsPlayerRoughlyAtObstacleCenter()
    {
        float playerZ = player.transform.position.z;
        float obstacleZ = currentObstacle.transform.position.z;

        float threshold = 0.2f; // you can tweak this (in meters)

        return Mathf.Abs(playerZ - obstacleZ) <= threshold;
    }

    public bool IsPlayerOnTopObstacle()
    {
        Collider playerCol = player.GetComponent<Collider>();
        Vector3 down = new Vector3(playerCol.bounds.center.x, playerCol.bounds.min.y + 0.1f, playerCol.bounds.center.z);

        int layerMask = ~LayerMask.GetMask("Character");
        if (Physics.Raycast(down, Vector3.down, out RaycastHit hit, 5f, layerMask))
        {
            GameObject newObstacle = GetParentObstacle(hit.collider.gameObject);

            if (newObstacle == currentObstacle)
            {
                return true;
            }
        }

        return false;
    }

    bool IsLegHoldObstacle(GameObject obj) => obj?.GetComponent<ObstacleBase>()?.ObstacleCode.StartsWith("LH") == true;

    bool IsCorrectLegHeld(ObstacleBase obstacle)
    {
        string code = obstacle.ObstacleCode;

        if (code.StartsWith("LHL")) return puppetAvatar.isLegHoldingLeft;
        if (code.StartsWith("LHR")) return puppetAvatar.isLegHoldingRight;

        return false;
    }

    void ResetLegHoldState()
    {
        legHoldTimer = 0f;
        hasPassedLegHold = false;
        requiredHoldTime = 0;

        if (legHoldRoutine != null)
        {
            StopCoroutine(legHoldRoutine);
            legHoldRoutine = null;
        }
    }

    public GameObject GetParentObstacle(GameObject obj)
    {
        GameObject current = obj;
        while (current.transform.parent != null)
        {
            if (current.transform.parent.gameObject == levelGenerator.gameObject)
                return current;
            current = current.transform.parent.gameObject;
        }
        return null;
    }

    void UpdateLevelStringProgress()
    {
        // Dynamic progress-tracking string
        string[] newCodes = newLevelString.Split(',');

        int playerIdx = System.Array.IndexOf(newCodes, "!");

        if (playerIdx < 0 || playerIdx >= newCodes.Length - 1) return;

        // Move "!" forward in the level string
        (newCodes[playerIdx], newCodes[playerIdx + 1]) =
            (newCodes[playerIdx + 1], newCodes[playerIdx]);

        newLevelString = string.Join(",", newCodes);

        // Also update the obstacles list so the order still matches
        var tmp = levelGenerator.Obstacles[playerIdx];
        levelGenerator.Obstacles[playerIdx] = levelGenerator.Obstacles[playerIdx + 1];
        levelGenerator.Obstacles[playerIdx + 1] = tmp;
    }

    public void AddToHoldTimer(float amount)
    {
        legHoldAccumulatedTime += amount;
    }
}
