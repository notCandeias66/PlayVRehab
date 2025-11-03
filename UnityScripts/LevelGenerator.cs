using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelGenerator : MonoBehaviour
{
    [SerializeField] private GameObject playerObject;

    [SerializeField] private Camera mainCamera;

    public bool DoneGenerating = false;

    public List<GameObject> Obstacles;

    [Header("Create your level:")]
    public string levelString;

    private Dictionary<string, GameObject> prefabDict = new Dictionary<string, GameObject>();
    private int currentZ = 0;

    void Start()
    {
        Obstacles = new List<GameObject>();

        LoadPrefabs();

        GetLevelSelectedString();
    }

    public void LoadPrefabs()
    {
        GameObject[] allPrefabs = Resources.LoadAll<GameObject>("Obstacles");

        foreach (var prefab in allPrefabs)
        {
            ObstacleBase obstacle = prefab.GetComponent<ObstacleBase>();
            if (obstacle != null)
            {
                string code = obstacle.ObstacleCode;
                if (!prefabDict.ContainsKey(code))
                {
                    prefabDict.Add(code, prefab);
                }
                else
                {
                    Debug.LogWarning($"Duplicate ObstacleCode detected: {code}");
                }
            }
        }
    }

    public void GetLevelSelectedString()
    {
        // Check if a level was selected before loading this scene
        if (UserSession.ChoosenLevel != null)
        {
            levelString = UserSession.ChoosenLevel["progression"].ToString();
            GenerateLevelFromString(levelString);
        }
        else
        {
            levelString = "!,LHLM5";
            GenerateLevelFromString(levelString);
        }
    }

    public void GenerateLevelFromString(string levelData)
    {
        currentZ = 0;

        // Do this so it generates the End obstacles adjacent to the other obstacles WITHOUT adding it to the level string
        levelData = levelData + ",END";

        string[] codes = levelData.Split(',');

        foreach (string rawCode in codes)
        {      
            string code = rawCode.Trim();

            // Extract obstacle code and delay number
            int i = code.Length - 1;
            while (i >= 0 && char.IsDigit(code[i])) i--;

            string obstacleCode = code.Substring(0, i + 1);
            string numberPart = code.Substring(i + 1);

            int delayAfter = 0;
            if (!string.IsNullOrEmpty(numberPart) && int.TryParse(numberPart, out int parsed))
            {
                delayAfter = parsed;
            }

            if (!prefabDict.TryGetValue(obstacleCode, out GameObject prefab))
            {
                Debug.LogWarning($"No prefab found for code: '{obstacleCode}'");
                continue;
            }

            ObstacleBase obstacle = prefab.GetComponent<ObstacleBase>();
            int length = obstacle != null ? obstacle.GetLength() : 1;

            // Spawn at currentZ + length/2 (center the obstacle on the z-axis)
            int spawnZ = currentZ + length / 2;

            GameObject obstacleInstance = Instantiate(prefab, new Vector3(0, 0, -spawnZ), Quaternion.Euler(0, 180f, 0), transform);

            Obstacles.Add(obstacleInstance);

            if (obstacle != null)
            {
                obstacleInstance.GetComponent<ObstacleBase>().SetDelay(delayAfter);
            }

            if (obstacleCode == "!")
            {
                Transform spawnPoint = obstacleInstance.transform.Find("SpawnPoint");

                if (spawnPoint != null && playerObject != null)
                {
                    // Move the existing player to the spawn point
                    playerObject.transform.position = spawnPoint.position + new Vector3(0, 0.9f, 0);

                    mainCamera.transform.position = new Vector3(0, playerObject.transform.position.y + 0.5f, playerObject.transform.position.z + 2.5f);

                    mainCamera.transform.rotation = Quaternion.Euler(0, 180, 0);
                }
                else
                {
                    Debug.LogWarning("Could not find 'SpawnPoint' or playerObject is missing.");
                }
            }

            // Advance currentZ by obstacle length plus delay AFTER the obstacle
            currentZ += length + delayAfter;
        }

        DoneGenerating = true;
    }
}
