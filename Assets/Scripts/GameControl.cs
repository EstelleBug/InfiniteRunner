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
    //private int consecutiveSameTypeCount = 0;
    //private int currentGroundTypeIndex = -1; // Declare currentGroundTypeIndex here
    //private string currentGroundTag = null;

    private GameObject Player;

    private int numberOfGrounds = 7;
    private int totalCoinCount = 0;

    private int numberOfCoins = 30;
    private int totalHitCoins = 0;
    private int previousTotalHitCoins = 0;

    private int numberOfObstacles = 20;
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

        for (int i = 0; i < numberOfGrounds; i++)
        {
            int n = Random.Range(0, GroundsPrefabs.Length);
            //UpdateGroundType();
            GroundsOnStage[i] = Instantiate(GroundsPrefabs[n]);
            //GroundsOnStage[i] = Instantiate(GroundsPrefabs[currentGroundTypeIndex]);
            SpawnCoins(GroundsOnStage[i]);
        }

        //groundSize = GroundsOnStage[0].GetComponentInChildren<Transform>().Find("Road").localScale.z;
        groundSize = 23.3f;

        float pos = Player.transform.position.z + groundSize / 2 - 1.5f;
        foreach (var ground in GroundsOnStage)
        {
            ground.transform.position = new Vector3(0, 0.2f, pos);
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
                float z = ground.transform.position.z;
                DestroyCoinsOnGround(ground);
                DestroyObstaclesOnGround(ground);
                Destroy(ground);
                int n = Random.Range(0, GroundsPrefabs.Length);
                ground = Instantiate(GroundsPrefabs[n]);
                // Check the tag of the new ground and adjust its position accordingly
                if (ground.CompareTag("Desert") || ground.CompareTag("Mountain"))
                {
                    ground.transform.position = new Vector3(0.6f, 0.2f, z + groundSize * numberOfGrounds);
                }
                else if (ground.CompareTag("Forest"))
                {
                    ground.transform.position = new Vector3(0, 0.2f, z + groundSize * numberOfGrounds);
                }

                //ground.transform.position = new Vector3(0, 0.2f, z + groundSize * numberOfGrounds);
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
            previousTotalHitCoins = totalHitCoins; // Mettre � jour le total pr�c�dent de pi�ces
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
        float coinSpawnXMin = 2f;
        float coinSpawnXMax = 4f;
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
        float obstacleSpawnZOffset = 10f; // Minimum distance between two obstacles
        float obstacleSpawnXMin = 2f;
        float obstacleSpawnXMax = 4f;

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

    /*private void UpdateGroundType()
    {
        int newGroundTypeIndex = Random.Range(0, GroundsPrefabs.Length);
        string newGroundTag = GroundsPrefabs[newGroundTypeIndex].tag;

        if (currentGroundTag == null || newGroundTag != currentGroundTag)
        {
            currentGroundTag = newGroundTag;
            consecutiveSameTypeCount = 1;
        }
        else
        {
            consecutiveSameTypeCount++;
            if (consecutiveSameTypeCount >= 6)
            {
                int currentIndex = newGroundTypeIndex;
                for (int i = 1; i < GroundsPrefabs.Length; i++)
                {
                    currentIndex = (currentIndex + 1) % GroundsPrefabs.Length;
                    if (GroundsPrefabs[currentIndex].tag != currentGroundTag)
                    {
                        currentGroundTag = GroundsPrefabs[currentIndex].tag;
                        consecutiveSameTypeCount = 1;
                        break;
                    }
                }
            }
        }
    }*/

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
