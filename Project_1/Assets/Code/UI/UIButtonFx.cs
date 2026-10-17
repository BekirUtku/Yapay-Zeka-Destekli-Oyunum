using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Tema butonlarinin hover/secili/basili animasyonu. Fare ustune gelince buton
/// secilir; boylece fare ve klavye (ok tuslari + Enter) ayni vurguyu kullanir.
/// Olceklenmemis zaman kullanir, oyun duraklatilmisken de calisir.
/// </summary>
[DisallowMultipleComponent]
public class UIButtonFx : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler,
    ISelectHandler, IDeselectHandler
{
    private Image background;
    private Color normalColor;
    private Color hoverColor;

    private bool hovered;
    private bool selected;
    private bool pressed;
    private float highlight;

    public void Init(Image bg, Color normal, Color hover)
    {
        background = bg;
        normalColor = normal;
        hoverColor = hover;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovered = false;
        pressed = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressed = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
    }

    void OnDisable()
    {
        // Panel kapanip tekrar acildiginda eski vurgu kalmasin.
        hovered = false;
        selected = false;
        pressed = false;
        highlight = 0f;
        transform.localScale = Vector3.one;
        if (background != null)
        {
            background.color = normalColor;
        }
    }

    void Update()
    {
        if (background == null)
        {
            return;
        }

        float dt = Time.unscaledDeltaTime;
        highlight = Mathf.MoveTowards(highlight, hovered || selected ? 1f : 0f, dt * 8f);
        background.color = Color.Lerp(normalColor, hoverColor, highlight);

        float targetScale = pressed ? 0.97f : Mathf.Lerp(1f, 1.025f, highlight);
        float k = 1f - Mathf.Exp(-20f * dt);
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * targetScale, k);
    }
}
