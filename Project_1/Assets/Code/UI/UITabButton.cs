using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Segmentli sekme cubugundaki tek bir sekme. Etkin sekme vurgu renginde dolu,
/// digerleri saydam; fare ustune gelince hafifce belirir.
/// </summary>
[DisallowMultipleComponent]
public class UITabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Image background;
    private TextMeshProUGUI label;
    private Action onClick;

    private bool active;
    private bool hovered;
    private float activeAmount;
    private float hoverAmount;

    public void Init(Image bg, TextMeshProUGUI text, Action clicked)
    {
        background = bg;
        label = text;
        onClick = clicked;
    }

    public void SetActive(bool isActive)
    {
        active = isActive;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }

    void OnDisable()
    {
        hovered = false;
        hoverAmount = 0f;
    }

    void Update()
    {
        if (background == null)
        {
            return;
        }

        float dt = Time.unscaledDeltaTime;
        activeAmount = Mathf.MoveTowards(activeAmount, active ? 1f : 0f, dt * 10f);
        hoverAmount = Mathf.MoveTowards(hoverAmount, hovered ? 1f : 0f, dt * 10f);

        Color idle = Color.Lerp(UITheme.WithAlpha(Color.white, 0f), UITheme.Secondary, hoverAmount);
        background.color = Color.Lerp(idle, UITheme.Accent, activeAmount);
        label.color = Color.Lerp(Color.Lerp(UITheme.TextSecondary, UITheme.TextPrimary, hoverAmount), UITheme.OnAccent, activeAmount);
    }
}
