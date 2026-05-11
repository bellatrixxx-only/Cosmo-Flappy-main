using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class StartScreenController : MonoBehaviour
{
    [Header("UI Ссылки")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI subtitleText;
    [SerializeField] private TextMeshProUGUI recordText;
    [SerializeField] private TextMeshProUGUI startHintText;

    [Header("Настройки")]
    [SerializeField] private float blinkSpeed = 2f;
    [SerializeField] private GameManager gameManager;

    private Color originalHintColor;

    void Start()
    {
        originalHintColor = startHintText.color;

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScore > 0)
        {
            string prefix = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText("your_record")
                : "Your Record";
            recordText.text = $"{prefix}: {highScore}";
            recordText.gameObject.SetActive(true);
        }
        else
        {
            recordText.gameObject.SetActive(false);
        }

        if (YandexManager.Instance != null)
            YandexManager.Instance.ShowStickyBanner();
    }
    private void UpdateRecordDisplay()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScore > 0)
        {
            string prefix = LocalizationManager.Instance != null
                ? LocalizationManager.Instance.GetText("your_record")
                : "Your Record";

            recordText.text = $"{prefix}: {highScore}";
            recordText.gameObject.SetActive(true);
        }
        else
        {
            recordText.gameObject.SetActive(false);
        }
    }

    public void OnLanguageChanged()
    {
        UpdateRecordDisplay();
    }

    void Update()
    {
        float alpha = Mathf.Sin(Time.time * blinkSpeed);
        startHintText.color = new Color(originalHintColor.r, originalHintColor.g, originalHintColor.b, Mathf.Abs(alpha));

        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) &&
            !EventSystem.current.IsPointerOverGameObject())
        {
            StartGame();
        }
    }

    void StartGame()
    {
        if (gameManager != null)
        {
            gameManager.StartTheGame();
            gameObject.SetActive(false);
        }
    }
}