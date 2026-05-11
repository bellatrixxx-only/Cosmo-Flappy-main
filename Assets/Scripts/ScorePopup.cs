using UnityEngine;
using TMPro;

public class ScorePopup : MonoBehaviour
{
    [Header("Настройки анимации")]
    public float lifeTime = 0.8f;      
    public float moveDistance = 50f;   
    public float fadeStartTime = 0.5f; 

    private RectTransform rectTransform;
    private TMP_Text textComponent; 
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        textComponent = GetComponent<TMP_Text>(); 
    }

    void OnEnable()
    {
        StartCoroutine(AnimatePopup());
    }

    private System.Collections.IEnumerator AnimatePopup()
    {
        float elapsedTime = 0f;
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos + Vector2.up * moveDistance;

        Color originalColor = textComponent.color;

        while (elapsedTime < lifeTime)
        {
          
            float progress = elapsedTime / lifeTime;
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);

            
            float alpha = 1f;
            if (elapsedTime > fadeStartTime)
            {
                float fadeProgress = (elapsedTime - fadeStartTime) / (lifeTime - fadeStartTime);
                alpha = Mathf.Lerp(1f, 0f, fadeProgress);
            }

            textComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}