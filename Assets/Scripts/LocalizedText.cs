using UnityEngine;
using TMPro;

public class LocalizedText : MonoBehaviour
{
    [SerializeField] private string localizationKey;
    private TextMeshProUGUI textComponent;

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();

        if (textComponent == null)
        {
            Debug.LogWarning($"[LocalizedText] TextMeshProUGUI component not found on {gameObject.name}");
            return;
        }

        Refresh();
    }

    public void Refresh()
    {
        if (textComponent != null && LocalizationManager.Instance != null && !string.IsNullOrEmpty(localizationKey))
        {
            textComponent.text = LocalizationManager.Instance.GetText(localizationKey);
        }
    }
}