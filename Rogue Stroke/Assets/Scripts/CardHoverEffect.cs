using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Intro Animation")]
    public float scaleUpDuration = 0.3f;
    public Vector3 startScale = Vector3.zero;
    public Vector3 finalScale = Vector3.one;

    [Header("Hover Settings")]
    public float hoverScaleMultiplier = 1.1f;
    public float liftHeight = 10f;
    public float transitionSpeed = 5f;

    private RectTransform rectTransform;
    private Vector3 targetScale;
    private Vector3 hoverScale;

    private Vector3 originalPosition;
    private Vector3 targetPosition;
    private bool isHovering = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rectTransform.localScale = startScale; // Start at custom start scale
    }

    void Start()
    {
        originalPosition = rectTransform.anchoredPosition;
        targetScale = finalScale;
        hoverScale = finalScale * hoverScaleMultiplier;
        targetPosition = originalPosition;

        StartCoroutine(ScaleIn());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        targetScale = hoverScale;
        targetPosition = originalPosition + Vector3.up * liftHeight;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        targetScale = finalScale;
        targetPosition = originalPosition;
    }

    void Update()
    {
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.deltaTime * transitionSpeed);
        rectTransform.anchoredPosition = Vector3.Lerp(rectTransform.anchoredPosition, targetPosition, Time.deltaTime * transitionSpeed);
    }

    private System.Collections.IEnumerator ScaleIn()
    {
        float elapsed = 0f;
        while (elapsed < scaleUpDuration)
        {
            float t = elapsed / scaleUpDuration;
            rectTransform.localScale = Vector3.Lerp(startScale, finalScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rectTransform.localScale = finalScale;
    }
}


