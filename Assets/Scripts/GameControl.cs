using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControl : MonoBehaviour
{
    public static GameControl Instance;

    [SerializeField]
    private GameObject[] GroundsPrefabs;
    [SerializeField]
    private GameObject[] GroundsOnStage;
    [SerializeField]
    private GameObject CoinPrefab;
    [SerializeField]
    private GameObject ObstaclePrefabs;
    [SerializeField]
    private GameObject ObstacleSlidePrefabs;

    private float groundSize;
    private int consecutiveSameTagCount = 7;
    private string currentTag = null;
    private int GroundIndex = 0;

    private GameObject Player;

    private int numberOfGrounds = 7;
    private int totalCoinCount = 0;

    private int numberOfCoins = 30;
    private int totalHitCoins = 0;
    private int previousTotalHitCoins = 0;

    private int numberOfObstacles = 10;
    private int totalObstaclesCount = 0;

    private List<GameObject> spawnedCoins = new List<GameObject>();
    private List<float> usedCoinZPositions = new List<float>();

    private List<GameObject> spawnedObstacles = new List<GameObject>();
    private List<float> usedObstacleZPositions = new List<float>();

    private int score = 0;
    private int newScore = 0;
    private int bonusScore = 0;

    public GameOverScreen GameOverScreen;
    public GameObject CoinCount;
    public GameObject ScoreCount;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }

        Instance = this;

        CoinCount = GameObject.Find("CoinCount");
        ScoreCount = GameObject.Find("ScoreCount");
    }

    // Start is called before the first frame update
    void Start()
    {
        Player = GameObject.Find("Ch03_nonPBR");

        GroundsOnStage = new GameObject[numberOfGrounds];

        int forestIndex = 0; // Index to track the current forest terrain

        for (int i = 0; i < numberOfGrounds; i++)
        {
            // Instantiate a Forest ground based on the current forestIndex
            GroundsOnStage[i] = Instantiate(GroundsPrefabs[GetForestIndices()[forestIndex]]);
            //consecutiveSameTagCount++;

            // Store the tag of the first terrain as the current tag
            if (currentTag == null)
            {
                currentTag = GroundsOnStage[i].tag;
            }

            // Move to the next forest terrain type
            forestIndex = (forestIndex + 1) % GetForestIndices().Count;
        }

        groundSize = 23.3f;

        float pos = Player.transform.position.z + groundSize / 2 - 1.5f;
        foreach (var ground in GroundsOnStage)
        {
            ground.transform.position = new Vector3(0f, 0.2f, pos);
            pos += groundSize;
        }
    }

    // Update is called once per frame
    void Update()
    {

        for (int i = GroundsOnStage.Length - 1; i >= 0; i--)
        {
            GameObject ground = GroundsOnStage[i];

            if (ground.transform.position.z + groundSize / 2 < Player.transform.position.z - 6f)
            {
                Debug.Log("NEW GROUND");
                Debug.Log(consecutiveSameTagCount);

                float z = ground.transform.position.z;
                DestroyCoinsOnGround(ground);
                DestroyObstaclesOnGround(ground);
                Destroy(ground);
                ground = Instantiate(GroundsPrefabs[AddNewGround()]);
                currentTag = ground.tag;

                // Calculate the new position
                if (ground.CompareTag("Desert") || ground.CompareTag("Mountain"))
                {
                    ground.transform.position = new Vector3(1f, 0.2f, z + groundSize * numberOfGrounds);
                }
                else if (ground.CompareTag("Forest"))
                {
                    ground.transform.position = new Vector3(0, 0.2f, z + groundSize * numberOfGrounds);
                }

                GroundsOnStage[i] = ground;
            }
        }

        foreach (var ground in GroundsOnStage)
        {
            //SpawnCoins(ground);
            //SpawnObstacle(ground);
        }

        UpdateScore();

    }

    private void UpdateScore()
    {

        int newHitCoins = totalHitCoins - previousTotalHitCoins;

        if (newHitCoins >= 100)
        {
            bonusScore = (newHitCoins / 100) * 1000 + bonusScore;
            previousTotalHitCoins = totalHitCoins; // Mettre à jour le total précédent de pièces
            GameControlUI.Instance.ShowMessage();
        }

        newScore = Mathf.FloorToInt(Player.transform.position.z) + bonusScore;

        if (newScore >= score)
        {
            score = newScore;
        }

    }

    private void SpawnCoins(GameObject ground)
    {
        float coinSpawnXMin = 2.5f;
        float coinSpawnXMax = 3f;
        float coinSpawnZOffset = 2f;
        float coinSpawnY = 1.5f; // Y position of the coins
        Quaternion coinRotation = Quaternion.Euler(90f, 0f, 0f); // 90 degrees rotation on X axis

        Vector3 groundPosition = ground.transform.position;
        //float groundScaleX = ground.transform.localScale.x;

        float startX = groundPosition.x - coinSpawnXMin;
        float endX = groundPosition.x + coinSpawnXMax;

        for (float x = startX; x <= endX; x++)
        {
            if (Random.Range(0f, 1f) < 0.2f && totalCoinCount < numberOfCoins) // Adjust the spawn probability
            {
                float coinSpawnZ = groundPosition.z + coinSpawnZOffset;

                // Check if the Z position is already used for a coin
                while (usedCoinZPositions.Contains(coinSpawnZ))
                {
                    coinSpawnZ += coinSpawnZOffset;
                }

                Vector3 coinSpawnPosition = new Vector3(x, coinSpawnY, coinSpawnZ);
                GameObject coin = Instantiate(CoinPrefab, coinSpawnPosition, coinRotation);
                spawnedCoins.Add(coin);
                usedCoinZPositions.Add(coinSpawnZ); // Mark the Z position as used
                totalCoinCount++;
                if (totalCoinCount >= numberOfCoins)
                    break;
            }
        }
    }

    private void SpawnObstacle(GameObject ground)
    {
        float obstacleSpawnZOffset = 100f; // Minimum distance between two obstacles
        float obstacleSpawnXMin = 2f;
        float obstacleSpawnXMax = 3f;

        Vector3 groundPosition = ground.transform.position;

        float startX = groundPosition.x - obstacleSpawnXMin;
        float endX = groundPosition.x + obstacleSpawnXMax;

        for (float x = startX; x <= endX; x++)
        {
            if (Random.Range(0f, 1f) < 0.2f && totalObstaclesCount < numberOfObstacles)
            {
                float obstacleSpawnZ = groundPosition.z + obstacleSpawnZOffset;

                Ray ray = new Ray(new Vector3(x, 5f, obstacleSpawnZ), Vector3.down);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Ground")))
                {
                    float obstacleSpawnY = hit.point.y;

                    while (usedObstacleZPositions.Contains(obstacleSpawnZ))
                    {
                        obstacleSpawnZ += obstacleSpawnZOffset;
                    }

                    int obstaclePrefabIndex = Random.Range(0, 2); // Choix entre 0 et 1
                    GameObject obstaclePrefab;

                    if (obstaclePrefabIndex == 0)
                    {
                        obstaclePrefab = ObstaclePrefabs;
                    }
                    else
                    {
                        obstaclePrefab = ObstacleSlidePrefabs;
                    }

                    // Check if the obstacle prefab is the special one
                    if (obstaclePrefab == ObstacleSlidePrefabs)
                    {
                        // Instantiate the special obstacle and apply the Y rotation
                        GameObject obstacle = Instantiate(obstaclePrefab, new Vector3(x, obstacleSpawnY, obstacleSpawnZ), Quaternion.Euler(0f, 90f, 0f));
                        spawnedObstacles.Add(obstacle);
                        usedObstacleZPositions.Add(obstacleSpawnZ);
                        totalObstaclesCount++;
                        if (totalObstaclesCount >= numberOfObstacles)
                            break;
                    }
                    else
                    {
                        // Instantiate regular obstacle without rotation
                        GameObject obstacle = Instantiate(obstaclePrefab, new Vector3(x, obstacleSpawnY, obstacleSpawnZ), Quaternion.identity);
                        spawnedObstacles.Add(obstacle);
                        usedObstacleZPositions.Add(obstacleSpawnZ);
                        totalObstaclesCount++;
                        if (totalObstaclesCount >= numberOfObstacles)
                            break;
                    }
                }
            }
        }
    }

    private void DestroyCoinsOnGround(GameObject ground)
    {
        for (int i = spawnedCoins.Count - 1; i >= 0; i--)
        {
            GameObject coin = spawnedCoins[i];
            if (coin.transform.position.z >= ground.transform.position.z - groundSize / 2 &&
                coin.transform.position.z <= ground.transform.position.z + groundSize / 2)
            {
                usedCoinZPositions.Remove(coin.transform.position.z); // Remove the Z position from the list
                spawnedCoins.RemoveAt(i);
                Destroy(coin);
                totalCoinCount--;
            }
        }
    }

    private void DestroyObstaclesOnGround(GameObject ground)
    {
        for (int i = spawnedObstacles.Count - 1; i >= 0; i--)
        {
            GameObject obstacle = spawnedObstacles[i];
            if (obstacle.transform.position.z >= ground.transform.position.z - groundSize / 2 &&
                obstacle.transform.position.z <= ground.transform.position.z + groundSize / 2)
            {
                usedObstacleZPositions.Remove(obstacle.transform.position.z);
                spawnedObstacles.RemoveAt(i);
                Destroy(obstacle);
                totalObstaclesCount--;
            }
        }
    }

    public void DestroyCoin(GameObject coin)
    {
        if (spawnedCoins.Contains(coin))
        {
            usedCoinZPositions.Remove(coin.transform.position.z); // Remove the Z position from the list
            spawnedCoins.Remove(coin);
            Destroy(coin);
            totalCoinCount--;

            if (coin.CompareTag("Reward"))
            {
                totalHitCoins++;
            }
        }
    }

    private int AddNewGround()
    {
        if (consecutiveSameTagCount >= 6 && consecutiveSameTagCount <= 10)
        {
            // Génère un nombre aléatoire entre 0 et 1
            float chance = Random.value;

            if (chance < 0.5f)
            {
                GroundIndex = ChangeTerrainTag();
                Debug.Log($"Chance : ChangeTag {GroundIndex}");
            }
            else
            {
                Debug.Log($"No Change Tag {currentTag}");
                consecutiveSameTagCount++;

                switch (currentTag)
                {
                    case "Forest":
                        GroundIndex = Random.Range(0, GetForestIndices().Count);
                        break;
                    case "Desert":
                        GroundIndex = Random.Range(0, GetDesertIndices().Count);
                        break;
                    case "Mountain":
                        GroundIndex = Random.Range(0, GetMountainIndices().Count);
                        break;
                }
            }
        }
        else if (consecutiveSameTagCount > 10)
        {
            GroundIndex = ChangeTerrainTag();
            Debug.Log($"Too many : ChangeTag {GroundIndex}");
        }
        else if (consecutiveSameTagCount < 6)
        {
            Debug.Log($"No Change Tag {currentTag}");
            consecutiveSameTagCount++;

            switch (currentTag)
            {
                case "Forest":
                    GroundIndex = Random.Range(0, GetForestIndices().Count);
                    break;
                case "Desert":
                    GroundIndex = Random.Range(0, GetDesertIndices().Count);
                    break;
                case "Mountain":
                    GroundIndex = Random.Range(0, GetMountainIndices().Count);
                    break;
            }
        }

            return GroundIndex;
    }

    private int ChangeTerrainTag()
    {
        int newTagIndex = Random.Range(0, GroundsPrefabs.Length);
        string newTag = GroundsPrefabs[newTagIndex].tag;

        // Avoid changing to the same tag
        if (newTag != currentTag)
        {
            currentTag = newTag;
            consecutiveSameTagCount = 1; // Reset count
        }
        else
        {
            consecutiveSameTagCount++;
        }
        return newTagIndex;
    }

    private List<int> GetForestIndices()
    {
        List<int> forestIndices = new List<int>();
        for (int i = 0; i < GroundsPrefabs.Length; i++)
        {
            if (GroundsPrefabs[i].CompareTag("Forest"))
            {
                forestIndices.Add(i);
            }
        }
        return forestIndices;
    }

    private List<int> GetDesertIndices()
    {
        List<int> desertIndices = new List<int>();
        for (int i = 0; i < GroundsPrefabs.Length; i++)
        {
            if (GroundsPrefabs[i].CompareTag("Desert"))
            {
                desertIndices.Add(i);
            }
        }
        return desertIndices;
    }

    private List<int> GetMountainIndices()
    {
        List<int> mountainIndices = new List<int>();
        for (int i = 0; i < GroundsPrefabs.Length; i++)
        {
            if (GroundsPrefabs[i].CompareTag("Mountain"))
            {
                mountainIndices.Add(i);
            }
        }
        return mountainIndices;
    }

    public int GetScore()
    {
        return score;
    }

    public int GetCoin()
    {
        return totalHitCoins;
    }

    private void OnDestroy()
    {
        Instance = null;
    }

    public void GameOver()
    {
        GameOverScreen.Setup(score);
        CoinCount.SetActive(false);
        ScoreCount.SetActive(false);
    }
}
