using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using YG;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Интерфейс")]
    [SerializeField] private GameObject hud;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private GameObject newRecordText;

    [Header("Спавн")]
    [SerializeField] private GameObject obstaclePairPrefab;
    [SerializeField] private GameObject meteoritePrefab;
    [SerializeField] private GameObject bonusPrefab;

    [Header("Настройки препятствий")]
    [SerializeField] private float startObstacleInterval = 2.5f;
    [SerializeField] private float minObstacleInterval = 0.7f;
    [SerializeField] private float obstacleIntervalStep = 0.1f;
    [SerializeField] private float gapSize = 2.5f;
    [SerializeField] private float spawnOffsetX = -1.5f;

    [Header("Метеориты")]
    [SerializeField] private float startMeteoriteInterval = 4f;
    [SerializeField] private float minMeteoriteInterval = 1.2f;
    [SerializeField] private float meteoriteIntervalStep = 0.3f;

    private int meteoriteSpawnCount = 1;
    private const int MAX_METEORITES_PER_WAVE = 3;
    private const float METEORITE_SPREAD = 1.2f;

    [Header("Бонусы")]
    [SerializeField] private float startBonusInterval = 15f;
    [SerializeField] private float maxBonusInterval = 40f;
    [SerializeField] private float bonusIntervalStep = 2f;

    [Header("Скорость")]
    [SerializeField] private float startGameSpeed = 3f;
    [SerializeField] private float speedStep = 0.4f;

    [Header("Фон")]
    [SerializeField] private List<ParallaxLayer> parallaxLayers;
    [SerializeField] private float bgSpeedRatio = 0.5f;

    private Camera mainCam;
    private PlayerController playerController;
    private float score = 0;
    private bool isGameActive = false;
    private bool isGameStarted = false;

    public bool IsGameStarted => isGameStarted;

    private float currentObstacleInterval;
    private float currentMeteoriteInterval;
    private float currentBonusInterval;
    private float currentGameSpeed;

    private float obstacleTimer;
    private float meteoriteTimer;
    private float bonusTimer;

    private readonly List<ObstaclePair> activeObstacles = new List<ObstaclePair>();
    private readonly List<GameObject> activePipes = new List<GameObject>();

    private const float OBSTACLE_CHECK_RANGE = 15f;
    private const float OBSTACLE_MIN_DIST = -3f;
    private const float SAFETY_MARGIN_SPAWN = 1.8f;
    private const float SAFETY_MARGIN_GAP = 0.5f;
    private const float COLLISION_RADIUS = 0.6f;
    private const int MAX_SPAWN_ATTEMPTS = 30;
    private const string TAG_PIPE = "Pipe";
    private const string TAG_OBSTACLE = "Obstacle";
    private const string NAME_TOP = "TopObstacle";
    private const string NAME_BOTTOM = "BottomObstacle";

    void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        mainCam = Camera.main;
        playerController = FindFirstObjectByType<PlayerController>();

        if (hud != null) hud.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (startScreen != null) startScreen.SetActive(true);
        if (scoreText != null) scoreText.text = "000000";

        ResetGameValues();
    }

    private void ResetGameValues()
    {
        score = 0;
        isGameActive = false;
        isGameStarted = false;

        currentGameSpeed = startGameSpeed;
        currentObstacleInterval = startObstacleInterval;
        currentMeteoriteInterval = startMeteoriteInterval;
        currentBonusInterval = startBonusInterval;

        obstacleTimer = currentObstacleInterval;
        meteoriteTimer = currentMeteoriteInterval;
        bonusTimer = currentBonusInterval;

        meteoriteSpawnCount = 1;

        activeObstacles.Clear();
        activePipes.Clear();

        UpdateBackgroundSpeed();
    }

    void Update()
    {
        if (!isGameStarted || !isGameActive) return;

        SpawnObstacle();
        SpawnMeteorite();
        SpawnBonus();
    }

    private float GetRightScreenEdge() => mainCam.orthographicSize * mainCam.aspect;

    private Vector2 GetSpawnPosition()
    {
        float spawnX = GetRightScreenEdge() + spawnOffsetX;
        float minY = -mainCam.orthographicSize + 1.5f;
        float maxY = mainCam.orthographicSize - 1.5f;
        return new Vector2(spawnX, Random.Range(minY, maxY));
    }

    private void UpdateObstacleCache()
    {
        activeObstacles.Clear();
        activeObstacles.AddRange(FindObjectsByType<ObstaclePair>(FindObjectsSortMode.None));
    }

    private Vector2 GetMeteoriteSpawnPosition(float horizontalOffset = 0f)
    {
        float spawnX = GetRightScreenEdge() + spawnOffsetX + horizontalOffset;
        float camHeight = mainCam.orthographicSize;
        float minY = -camHeight + 1f;
        float maxY = camHeight - 1f;

        
        float safetyMargin = 2.2f;  

        ObstaclePair[] pairs = FindObjectsByType<ObstaclePair>(FindObjectsSortMode.None);
        ObstaclePair nearestPair = null;
        float minDist = float.MaxValue;

        foreach (var pair in pairs)
        {
            float dist = pair.transform.position.x - spawnX;
            if (dist > -3f && dist < 15f && dist < minDist)
            {
                minDist = dist;
                nearestPair = pair;
            }
        }

        if (nearestPair != null)
        {
            float topMaxY = float.MinValue;
            float bottomMinY = float.MaxValue;

            foreach (Transform child in nearestPair.transform)
            {
                if (child.name.Contains("TopObstacle"))
                {
                    Collider2D col = child.GetComponent<Collider2D>();
                    if (col != null) topMaxY = col.bounds.max.y;
                }
                else if (child.name.Contains("BottomObstacle"))
                {
                    Collider2D col = child.GetComponent<Collider2D>();
                    if (col != null) bottomMinY = col.bounds.min.y;
                }
            }

            if (topMaxY > float.MinValue && bottomMinY < float.MaxValue)
            {
                if (Random.value < 0.5f)
                {
                    float spawnY = topMaxY + safetyMargin;
                    spawnY = Mathf.Clamp(spawnY, topMaxY + safetyMargin, maxY);
                    return new Vector2(spawnX, spawnY);
                }
                else
                {
                    float spawnY = bottomMinY - safetyMargin;
                    spawnY = Mathf.Clamp(spawnY, minY, bottomMinY - safetyMargin);
                    return new Vector2(spawnX, spawnY);
                }
            }
        }

        return new Vector2(spawnX, Random.Range(minY, maxY));
    }

    private void UpdatePipeCache()
    {
        activePipes.Clear();
        GameObject[] found = GameObject.FindGameObjectsWithTag(TAG_PIPE);
        activePipes.AddRange(found);
    }

    private Vector2 GetSafeSpawnPosition(float minY, float maxY)
    {
        if (activePipes.Count == 0) UpdatePipeCache();

        int attempts = 0;
        LayerMask obstacleLayer = LayerMask.GetMask(TAG_OBSTACLE);

        do
        {
            float spawnX = GetRightScreenEdge() + spawnOffsetX;
            float spawnY = Random.Range(minY, maxY);
            Vector2 spawnPos = new Vector2(spawnX, spawnY);

            if (Physics2D.OverlapCircle(spawnPos, COLLISION_RADIUS, obstacleLayer) != null)
            {
                attempts++;
                continue;
            }

            bool isInsideGap = false;
            foreach (var pipeObj in activePipes)
            {
                if (pipeObj == null || pipeObj.transform.position.x > -5f) continue;

                Collider2D topCol = null, botCol = null;

                foreach (Transform child in pipeObj.transform)
                {
                    if (child.name.Contains(NAME_TOP)) topCol = child.GetComponent<Collider2D>();
                    else if (child.name.Contains(NAME_BOTTOM)) botCol = child.GetComponent<Collider2D>();
                }

                if (topCol != null && botCol != null)
                {
                    if (spawnY > botCol.bounds.max.y - SAFETY_MARGIN_GAP && spawnY < topCol.bounds.min.y + SAFETY_MARGIN_GAP)
                    {
                        isInsideGap = true;
                        break;
                    }
                }
            }

            if (!isInsideGap) return spawnPos;
            attempts++;
        } while (attempts < MAX_SPAWN_ATTEMPTS);

        return new Vector2(GetRightScreenEdge() + spawnOffsetX, Random.Range(minY, maxY));
    }

    void SpawnObstacle()
    {
        obstacleTimer -= Time.deltaTime;
        if (obstacleTimer <= 0)
        {
            Vector2 pos = GetSpawnPosition();
            GameObject pair = Instantiate(obstaclePairPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);

            foreach (Transform child in pair.GetComponentsInChildren<Transform>())
            {
                if (child.name.Contains(NAME_TOP)) child.localPosition = new Vector3(0, gapSize / 2 + 2.5f, 0);
                else if (child.name.Contains(NAME_BOTTOM)) child.localPosition = new Vector3(0, -gapSize / 2 - 2.5f, 0);
            }

            if (pair.TryGetComponent<ObstaclePair>(out var pairScript))
            {
                pairScript.Init(currentGameSpeed);
            }

            obstacleTimer = currentObstacleInterval;

            UpdateObstacleCache();
        }
    }

    void SpawnMeteorite()
    {
        if (score < 5) return;

        meteoriteTimer -= Time.deltaTime;
        if (meteoriteTimer <= 0)
        {
            for (int i = 0; i < meteoriteSpawnCount; i++)
            {
                float spread = i * METEORITE_SPREAD;
                Vector2 pos = GetMeteoriteSpawnPosition(spread);
                GameObject meteorite = Instantiate(meteoritePrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);

                if (meteorite.TryGetComponent<Meteorite>(out var mScript))
                {
                    mScript.SetSpeed(currentGameSpeed);
                }
            }

            meteoriteTimer = currentMeteoriteInterval;
        }
    }

    void SpawnBonus()
    {
        if (score < 5) return;

        bonusTimer -= Time.deltaTime;
        if (bonusTimer <= 0)
        {
            Vector2 pos = GetSafeSpawnPosition(-mainCam.orthographicSize + 1f, mainCam.orthographicSize - 1f);
            GameObject bonus = Instantiate(bonusPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);

            if (bonus.TryGetComponent<BonusController>(out var bonusScript))
            {
                bonusScript.SetSpeed(currentGameSpeed);
            }

            bonusTimer = currentBonusInterval;
        }
    }

    public void StartTheGame()
    {
        if (YandexManager.Instance != null)
        {
            YandexManager.Instance.NotifyGameStart();
          
            YandexManager.Instance.HideStickyBanner();
        }

        enabled = true;

        score = 0;
        if (scoreText != null) scoreText.text = "000000";

        isGameStarted = true;
        isGameActive = true;

        if (startScreen != null) startScreen.SetActive(false);
        if (hud != null) hud.SetActive(true);

        if (playerController != null)
        {
            playerController.StartPlaying();
        }
    }

    public void AddScore()
    {
        if (!isGameActive) return;

        score++;
        if (scoreText != null) scoreText.text = ((int)score).ToString("000000");

        if ((int)score % 10 == 0 && score >= 10)
        {
            IncreaseDifficulty();
        }
    }

    void IncreaseDifficulty()
    {
        currentGameSpeed += speedStep;
        currentObstacleInterval = Mathf.Max(minObstacleInterval, currentObstacleInterval - obstacleIntervalStep);
        currentMeteoriteInterval = Mathf.Max(minMeteoriteInterval, currentMeteoriteInterval - meteoriteIntervalStep);
        currentBonusInterval = Mathf.Min(maxBonusInterval, currentBonusInterval + bonusIntervalStep);

        meteoriteSpawnCount = Mathf.Min(MAX_METEORITES_PER_WAVE, meteoriteSpawnCount + 1);

        UpdateBackgroundSpeed();
    }

    void UpdateBackgroundSpeed()
    {
        float bgSpeed = -currentGameSpeed * bgSpeedRatio;
        foreach (var layer in parallaxLayers) layer?.SetSpeed(bgSpeed);
    }

    public void ShowFinalScoreAnimation(int score)
    {
        string prefix = LocalizationManager.Instance != null
            ? LocalizationManager.Instance.GetText("your_score")
            : "Your Score:";
        StartCoroutine(SlotMachineAnimation(score, finalScoreText, prefix + " "));
    }

    public void ShowHighScoreAnimation(int highScore)
    {
        string prefix = LocalizationManager.Instance != null
            ? LocalizationManager.Instance.GetText("your_best")
            : "Your Best:";
        StartCoroutine(SlotMachineAnimation(highScore, highScoreText, prefix + " "));
    }

    private IEnumerator SlotMachineAnimation(int targetScore, TextMeshProUGUI textComponent, string prefix)
    {
        if (textComponent == null) yield break;

        float totalDuration = 1f;
        float elapsedTime = 0f;
        float randomChangeInterval = 0.05f;

        while (elapsedTime < totalDuration)
        {
            textComponent.text = prefix + Random.Range(0, 999999).ToString("D6");
            elapsedTime += randomChangeInterval;
            yield return new WaitForSeconds(randomChangeInterval);
        }
        yield return StartCoroutine(SmoothSettleToTarget(targetScore, textComponent, prefix));
    }

    private IEnumerator SmoothSettleToTarget(int targetScore, TextMeshProUGUI textComponent, string prefix)
    {
        float settleDuration = 0.3f;
        float elapsed = 0f;

        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;
            textComponent.text = prefix + Mathf.RoundToInt(Mathf.Lerp(999999, targetScore, elapsed / settleDuration)).ToString("D6");
            yield return null;
        }
        textComponent.text = prefix + targetScore.ToString("D6");
    }

    private IEnumerator DelayedHighScoreAnimation(int bestScore)
    {
        yield return new WaitForSeconds(0.3f);
        ShowHighScoreAnimation(bestScore);
    }

    public void GameOver()
    {
        if (!isGameActive) return;
        isGameActive = false;

        if (AudioManager.Instance != null) AudioManager.Instance.StopMusic();

        if (playerController != null)
        {
            if (playerController.rb != null) playerController.rb.linearVelocity = Vector2.zero;
            Destroy(playerController.gameObject);
        }

        foreach (var bg in FindObjectsByType<ParallaxLayer>(FindObjectsSortMode.None))
        {
            bg.enabled = false;
        }

        int currentScore = (int)score;
        int previousBest = PlayerPrefs.GetInt("HighScore", 0);
        int bestScore = Mathf.Max(currentScore, previousBest);
        bool isNewRecord = currentScore > previousBest;

        if (YandexManager.Instance != null)
        {
            YandexManager.Instance.NotifyGameEnd(currentScore);
        }

        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        if (isNewRecord)
        {
            PlayerPrefs.SetInt("HighScore", bestScore);
            PlayerPrefs.Save();

            if (finalScoreText != null) finalScoreText.gameObject.SetActive(false);
            if (highScoreText != null) highScoreText.gameObject.SetActive(false);

            StartCoroutine(PlayNewRecordAnimation(bestScore));
        }
        else
        {
            if (newRecordText != null) newRecordText.SetActive(false);
            if (finalScoreText != null) finalScoreText.gameObject.SetActive(true);
            if (highScoreText != null) highScoreText.gameObject.SetActive(true);

            ShowFinalScoreAnimation(currentScore);
            StartCoroutine(DelayedHighScoreAnimation(bestScore));
        }

        if (hud != null) hud.SetActive(false);
        if (scoreText != null) scoreText.text = "000000";

        if (YandexManager.Instance != null)
        {
            YandexManager.Instance.ShowStickyBanner();
        }
    }

    public void RestartGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.RestartMusic();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator PlayNewRecordAnimation(int targetScore)
    {
        if (newRecordText == null) yield break;

        TextMeshProUGUI tmp = newRecordText.GetComponent<TextMeshProUGUI>();
        newRecordText.SetActive(true);

        string newRecordKey = LocalizationManager.Instance != null
            ? LocalizationManager.Instance.GetText("new_record")
            : "NEW RECORD!";

        float slotDuration = 1.5f;
        float elapsed = 0f;
        float randomChangeInterval = 0.05f;
        float slotTimer = 0f;
        int currentDisplayScore = 0;

        while (elapsed < slotDuration)
        {
            elapsed += Time.deltaTime;
            slotTimer += Time.deltaTime;

            if (slotTimer >= randomChangeInterval)
            {
                slotTimer = 0f;
                currentDisplayScore = Random.Range(0, 999999);
            }

            tmp.text = $"{newRecordKey}\n{currentDisplayScore:000000}";
            yield return null;
        }

        float settleDuration = 0.5f;
        elapsed = 0f;
        int startScore = currentDisplayScore;

        while (elapsed < settleDuration)
        {
            elapsed += Time.deltaTime;
            currentDisplayScore = Mathf.RoundToInt(Mathf.Lerp(startScore, targetScore, elapsed / settleDuration));
            tmp.text = $"{newRecordKey}\n{currentDisplayScore:000000}";
            yield return null;
        }
        tmp.text = $"{newRecordKey}\n{targetScore:000000}";
    }
}