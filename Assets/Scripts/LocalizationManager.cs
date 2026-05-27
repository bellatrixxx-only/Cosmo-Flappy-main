using UnityEngine;
using TMPro;
using System.Collections.Generic;
using YG;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance;

    public enum Language { Russian, English }
    public Language currentLanguage = Language.Russian;

    [System.Serializable]
    public class TextEntry
    {
        public string key;
        public string ru;
        public string en;
    }

    [SerializeField] private List<TextEntry> texts = new List<TextEntry>();
    private Dictionary<string, TextEntry> textDict = new Dictionary<string, TextEntry>();

    private const string LANG_KEY = "GameLanguage";
    private const string FIRST_LAUNCH_KEY = "IsFirstLaunch";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

           
            YG2.onGetSDKData += OnYandexDataLoaded;

            if (YG2.isSDKEnabled)
            {
                OnYandexDataLoaded();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        YG2.onGetSDKData -= OnYandexDataLoaded;
    }

    private void OnYandexDataLoaded()
    {
        InitializeLanguage();
    }

    private void InitializeLanguage()
    {
        bool isFirstLaunch = PlayerPrefs.GetInt(FIRST_LAUNCH_KEY, 1) == 1;

        if (isFirstLaunch)
        {
            string sdkLang = YG2.envir.language;
            Debug.Log($"[YG] Auto-detected language: {sdkLang}");

            currentLanguage = ConvertLanguage(sdkLang);

            PlayerPrefs.SetInt(LANG_KEY, (int)currentLanguage);
            PlayerPrefs.SetInt(FIRST_LAUNCH_KEY, 0);
            PlayerPrefs.Save();
        }
        else
        {
            LoadLanguage();
        }

        BuildDictionary();
        ApplyLanguageToUI();
    }

    private Language ConvertLanguage(string sdkLang)
    {
        if (string.IsNullOrEmpty(sdkLang)) return Language.Russian;

        string lang = sdkLang.ToLower();

        if (lang.StartsWith("ru")) return Language.Russian;
        if (lang.StartsWith("en")) return Language.English;

        return Language.Russian;
    }

    private void BuildDictionary()
    {
        textDict.Clear();
        foreach (var entry in texts)
        {
            if (!string.IsNullOrEmpty(entry.key))
                textDict[entry.key] = entry;
        }
    }

    public string GetText(string key)
    {
        if (!textDict.TryGetValue(key, out var entry)) return key;
        return currentLanguage == Language.Russian ? entry.ru : entry.en;
    }

    public void SetLanguage(Language lang)
    {
        currentLanguage = lang;
        PlayerPrefs.SetInt(LANG_KEY, (int)lang);
        PlayerPrefs.Save();
        ApplyLanguageToUI();
    }

    private void LoadLanguage()
    {
        int saved = PlayerPrefs.GetInt(LANG_KEY, 0);
        currentLanguage = (Language)saved;
    }

    private void ApplyLanguageToUI()
    {
        foreach (var lt in FindObjectsByType<LocalizedText>(FindObjectsInactive.Exclude))
        {
            lt.Refresh();
        }
    }

    public void ToggleLanguage()
    {
        SetLanguage(currentLanguage == Language.Russian ? Language.English : Language.Russian);
    }
}