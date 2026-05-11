using UnityEngine;
using TMPro;

public class LanguageToggleButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buttonText;

    private void Start()
    {
        UpdateButtonText();
    }

    public void ToggleLanguage()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.ToggleLanguage();
            UpdateButtonText();

            StartScreenController startScreen = FindFirstObjectByType<StartScreenController>();
            if (startScreen != null)
            {
                startScreen.OnLanguageChanged();
            }
        }
    }

    private void UpdateButtonText()
    {
        if (LocalizationManager.Instance != null && buttonText != null)
        {
            buttonText.text = LocalizationManager.Instance.currentLanguage == LocalizationManager.Language.Russian
                ? "ENG"
                : "RUS";
        }
    }
}