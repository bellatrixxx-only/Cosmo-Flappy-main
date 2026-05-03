using UnityEngine;
using UnityEngine.UI;

public class CameraBorder : MonoBehaviour
{
    [Header("Настройки рамки")]
    [SerializeField] private Color borderColor = Color.magenta;
    [SerializeField] private float borderThickness = 5f;
    [SerializeField] private Camera targetCamera;

    private bool isInitialized = false;

    void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

    
        bool isDesktopMode = targetCamera.pixelRect.width < Screen.width;

        if (!isDesktopMode)
        {
            if (gameObject.activeSelf)
                gameObject.SetActive(false); 
            return;
        }

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (!isInitialized)
        {
            RectTransform rect = GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            isInitialized = true;
        }

        Rect camRect = targetCamera.pixelRect;
        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;

        float offsetX = camRect.center.x - centerX;
        float offsetY = camRect.center.y - centerY;

        
        UpdateBar("BorderTop", camRect.width, borderThickness, offsetX, offsetY + camRect.height / 2f - borderThickness / 2f);
        UpdateBar("BorderBottom", camRect.width, borderThickness, offsetX, offsetY - camRect.height / 2f + borderThickness / 2f);
        UpdateBar("BorderLeft", borderThickness, camRect.height, offsetX - camRect.width / 2f + borderThickness / 2f, offsetY);
        UpdateBar("BorderRight", borderThickness, camRect.height, offsetX + camRect.width / 2f - borderThickness / 2f, offsetY);
    }

    private void UpdateBar(string name, float width, float height, float posX, float posY)
    {
        Transform child = transform.Find(name);
        Image barImage;

        if (child == null)
        {
            GameObject barObj = new GameObject(name);
            barObj.transform.SetParent(transform, false);
            barImage = barObj.AddComponent<Image>();
            barImage.raycastTarget = false;
        }
        else
        {
            barImage = child.GetComponent<Image>();
        }

        barImage.color = borderColor;
        RectTransform barRect = barImage.rectTransform;

        barRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        barRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        barRect.anchoredPosition = new Vector2(posX, posY);
    }
}