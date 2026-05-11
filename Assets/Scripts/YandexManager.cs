using UnityEngine;
using YG;

public class YandexManager : MonoBehaviour
{
    public static YandexManager Instance;

    private const string LEADERBOARD_NAME = "cosmo_flappy_leaderboard";
    private const string SESSIONS_KEY = "Yandex_GameSessions";
    private const int AD_INTERVAL = 7;
    private int currentSessions = 0;
   
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        YG2.onGetSDKData += OnSdkInitialized;
        if (YG2.isSDKEnabled)
        {
            OnSdkInitialized();
        }
    }

    private void OnSdkInitialized()
    {
        YG2.onGetSDKData -= OnSdkInitialized;
        currentSessions = PlayerPrefs.GetInt(SESSIONS_KEY, 0);

        if (YG2.isFirstGameSession)
        {
            ShowInterstitialAd();
        }
    }

    public void NotifyGameStart()
    {
        YG2.GameplayStart();
    }

    public void NotifyGameEnd(int score)
    {
        YG2.GameplayStop();
        currentSessions++;
        PlayerPrefs.SetInt(SESSIONS_KEY, currentSessions);
        PlayerPrefs.Save();
        SaveLeaderboard(score);

        if (currentSessions > 0 && currentSessions % AD_INTERVAL == 0)
        {
            ShowInterstitialAd();
        }
    }

    public void ShowStickyBanner()
    {
        if (YG2.isSDKEnabled)
        {
            YG2.StickyAdActivity(true);
        }
    }

    public void HideStickyBanner()
    {
        if (YG2.isSDKEnabled)
        {
            YG2.StickyAdActivity(false);
        }
    }

    private void ShowInterstitialAd()
    {
        if (YG2.isSDKEnabled)
        {
            YG2.InterstitialAdvShow();
        }
    }

    private void SaveLeaderboard(int score)
    {
        if (YG2.isSDKEnabled)
        {
            int bestScore = PlayerPrefs.GetInt("BestScore", 0);
            if (score > bestScore)
            {
                YG2.SetLeaderboard(LEADERBOARD_NAME, score);
                PlayerPrefs.SetInt("BestScore", score);
                PlayerPrefs.Save();
            }
        }
    }

    private void OnDestroy()
    {
        if (YG2.isSDKEnabled)
        {
            YG2.StickyAdActivity(false);
        }
    }
}