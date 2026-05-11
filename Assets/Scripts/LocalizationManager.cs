using UnityEngine;
using TMPro;
using System.Collections.Generic;

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLanguage();
            BuildDictionary();
            ApplyLanguageToUI();
        }
        else
        {
            Destroy(gameObject);
        }
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
        foreach (var lt in FindObjectsByType<LocalizedText>(FindObjectsSortMode.None))
        {
            lt.Refresh();
        }
    }

    public void ToggleLanguage()
    {
        SetLanguage(currentLanguage == Language.Russian ? Language.English : Language.Russian);
    }
}